using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Server.Data;
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

        public UsersController(ServerContext db, IMapper mapper, UserManager<User> userManager)
        {
            this.db = db;
            this.mapper = mapper;
            this.userManager = userManager;
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

        // DELETE api/<UserController>/5
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
    }
}
