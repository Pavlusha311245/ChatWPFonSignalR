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
    }
}
