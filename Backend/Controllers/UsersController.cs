using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Hubs;
using Server.Models;
using Server.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        public ServerContext db;
        public IMapper mapper;
        public UserManager<User> userManager;
        public IHubContext<ChatHub> hubContext;

        public UsersController(ServerContext db,
            IMapper mapper,
            UserManager<User> userManager,
            IHubContext<ChatHub> hubContext)
        {
            this.db = db;
            this.mapper = mapper;
            this.userManager = userManager;
            this.hubContext = hubContext;
        }

        // GET: api/<UserController>
        [HttpGet]
        public IEnumerable<UserViewModel> Get()
        {
            return db.Users.Select(user => mapper.Map<User, UserViewModel>(user)).ToList();
        }

        // GET api/<UserController>/5
        [HttpGet("{id}")]
        public async Task<UserViewModel> Get(string id)
        {
            return mapper.Map<User, UserViewModel>(await userManager.FindByIdAsync(id));
        }

        // POST api/<UserController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<UserController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {

        }

        /// <summary>
        /// Delete user
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                await userManager.DeleteAsync(await userManager.FindByIdAsync(id));
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(new
                {
                    Error = "Not found " + ex.ParamName
                });
            }

            return Ok(new { Message = "User successfully deleted" });
        }

        
        /// <summary>
        /// This method add user to group chat
        /// </summary>
        /// <param name="groupname"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("{id}/groups")]
        public async Task<IActionResult> AddToGroup([FromBody] GroupChat groupname, string id)
        {
            var user = await userManager.FindByIdAsync(id);
            var chatGroup = await db.GroupChats.FirstOrDefaultAsync(c => c.Name == groupname.Name);

            try
            {
                chatGroup.Users.Add(user);
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(new
                {
                    Message = "User already exist in chat"
                });
            }

            await db.SaveChangesAsync();
            await hubContext.Clients.User(user.Email).SendAsync("NotifyAddToChat", "User successfully add to chat");

            return Ok(new
            {
                Message = $"User successfully add to chat {groupname.Name}"
            });
        }

        /// <summary>
        /// This method remove user from group chat
        /// </summary>
        /// <param name="groupname"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}/groups")]
        public async Task<IActionResult> RemoveFromGroup([FromBody] GroupChat groupname, string id)
        {
            var user = await userManager.FindByIdAsync(id);
            var chatGroup = await db.GroupChats.Include(c => c.Users).FirstOrDefaultAsync(c => c.Name == groupname.Name);

            if (chatGroup == null)
            {
                return NotFound(new
                {
                    Message = "User not found in choosed chat"
                });
            }

            chatGroup.Users.Remove(user);
            await hubContext.Clients.User(user.Email).SendAsync("NotifyRemoveFromChat", "User removed from chat");

            await db.SaveChangesAsync();

            return Ok(new
            {
                Message = $"User successfully removed from chat {groupname.Name}"
            });
        }
    }
}
