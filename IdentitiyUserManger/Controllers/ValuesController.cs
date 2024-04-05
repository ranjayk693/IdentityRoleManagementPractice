using IdentitiyUserManger.Data;
using IdentitiyUserManger.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentitiyUserManger.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public ValuesController(SignInManager<IdentityUser> signIn, UserManager<IdentityUser> userManager , RoleManager<IdentityRole> roleManager)
        {
            _signInManager = signIn;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        //[HttpPost]
        //public async Task<IActionResult> CreateUserAsync(Login user)
        //{
        //    //var data=new IdentityUser(Guid.NewGuid().ToString(),"");
        //    if (string.IsNullOrEmpty(user.email))
        //    {
        //        return NotFound("Not found data");
        //    }

        //    var email = new IdentityUser(user.email);

        //    var result = await _userManager.CreateAsync(email, user.password);
        //    return Ok(result); // Returning result without any conditional checks
        //}


        /*Creating user and assign user*/
        [HttpPost("User")]
        public async Task<IActionResult> CreateUserAsync(Login user)
        {
            if (string.IsNullOrEmpty(user.email))
            {
                return NotFound("Not found data");
            }
            var newRole = new IdentityRole("Admin");

            var newUser = new IdentityUser
            {
                UserName = user.email,
                Email = user.email // Setting both UserName and Email to the provided email address
            };
            await _userManager.CreateAsync(newUser, user.password);

            await _userManager.AddToRoleAsync(newUser, "Admin");

            return Ok("Created user sucessfully"); // Returning result without any conditional checks
        }

        [Authorize(Roles ="Admin")]
        [HttpGet("Test")]
        public async Task<IActionResult> Testing(string email)
        {

            //var user = new IdentityUser
            //{
            //    UserName=email
            //};
            //var result=await  _userManager.GetRolesAsync(user);
            //var data = await _userManager.FindByNameAsync(email);
            //return Ok(result);

            var user = await _userManager.FindByNameAsync(email);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var roles = await _userManager.GetRolesAsync(user);
            if (roles == null || !roles.Any())
            {
                return Ok("User exists but not assigned any roles.");
            }

            return Ok(user );
        }



        [HttpPost("Role")]
        public async Task<IActionResult> CreateRoleAsync()
        {
            var existingRole = await _roleManager.FindByNameAsync("Admin");
            if (existingRole != null)
            {
                return BadRequest("Role already exists.");
            }

            // Create the new role
            var newRole = new IdentityRole("Admin");
            var result = await _roleManager.CreateAsync(newRole);

            // Check if the role creation was successful
            if (result.Succeeded)
            {
                return Ok("Role created successfully.");
            }
            else
            {
                // If role creation failed, return error messages
                var errors = string.Join("\n", result.Errors.Select(e => e.Description));
                return BadRequest($"Failed to create role: {errors}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SignIn(SigninModel model)
        {
            var result = await _signInManager.PasswordSignInAsync(model.email, model.Password, true, lockoutOnFailure: false);
            if (result.Succeeded)
            {

                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }

        [HttpGet("SignOut")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok("Logout sucessfully");
        }
    }
}
