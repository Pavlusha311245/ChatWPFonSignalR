using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Server.Data;
using Server.Models;
using Server.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        public ServerContext db;
        public IMapper mapper;

        public UsersController(ServerContext db, IMapper mapper)
        {
            this.db = db;
            this.mapper = mapper;
        }

        // GET: api/<UserController>
        [HttpGet]
        public IEnumerable<UserViewModel> Get()
        {
            return db.Users.Select(user => mapper.Map<User, UserViewModel>(user)).ToList();
        }

        // GET api/<UserController>/5
        [HttpGet("{id}")]
        public UserViewModel Get(string id)
        {
            return mapper.Map<User, UserViewModel>(db.Users.FirstOrDefault(user => user.Id == id));
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
        public void Delete(int id)
        {
        }
    }
}
