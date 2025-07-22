using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRNLibrary.Services;

namespace LibararyWebApplication.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserInfoController : ControllerBase
    {
        private readonly PrnContext ctx;
        private readonly EmailService emailService;
        public UserInfoController(PrnContext context, EmailService emailService)
        {
            this.emailService = emailService;
            ctx = context;
        }

        public class SampleBookWC_Category
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class SampleBookWC
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public int AuthorId { get; set; }
            public string ImageBase64 { get; set; }
            public DateTime PublishedDate { get; set; }
            public virtual ICollection<SampleBookWC_Category> Categories { get; set; }

        }

        [HttpGet("/api/tung/books/add1c/{id}")]
        public IActionResult sample_books_add_cat([FromRoute] int id)
        {
            var l1 = ctx.Books.Include(b => b.Categories).Where(b => b.Id == id).FirstOrDefault();
            if (l1 == null)
            {
                return NotFound();
            }

            if (l1.Categories.Count == 0)
            {
                return NotFound();
            }

            var tmp_cat_id_list = l1.Categories.Select(c => c.Id).ToList();

            var cat_list = ctx.Categories.ToList();
            var free_cat_list = cat_list.Where(c => !tmp_cat_id_list.Contains(c.Id)).ToList();

            if (free_cat_list.Count == 0)
            {
                return NotFound();
            }

            l1.Categories.Add(free_cat_list.First());
            ctx.Books.Update(l1);
            ctx.SaveChanges();

            var book = l1;

            var c_list = book.Categories.Select(c => new SampleBookWC_Category()
            {
                Id = c.Id,
                Name = c.Name
            }).ToList();
            var b1 = new SampleBookWC()
            {
                Id = book.Id,
                Title = book.Title,
                AuthorId = book.AuthorId,
                ImageBase64 = book.ImageBase64,
                PublishedDate = book.PublishedDate,
                Categories = c_list
            };
            return Ok(b1);
        }



        [HttpGet("/api/tung/books/delete1c/{id}")]
        public IActionResult sample_books_del_cat([FromRoute] int id)
        {
            var l1 = ctx.Books.Include(b => b.Categories).Where(b => b.Id == id).FirstOrDefault();
            if (l1 == null)
            {
                return NotFound();
            }

            if (l1.Categories.Count == 0)
            {
                return NotFound();
            }

            l1.Categories.Remove(l1.Categories.First());
            ctx.Books.Update(l1);
            ctx.SaveChanges();

            var book = l1;

            var c_list = book.Categories.Select(c => new SampleBookWC_Category()
            {
                Id = c.Id,
                Name = c.Name
            }).ToList();
            var b1 = new SampleBookWC()
            {
                Id = book.Id,
                Title = book.Title,
                AuthorId = book.AuthorId,
                ImageBase64 = book.ImageBase64,
                PublishedDate = book.PublishedDate,
                Categories = c_list
            };
            return Ok(b1);
        }


        [HttpGet("/api/tung/books")]
        public IActionResult sample_books()
        {
            var l1 = ctx.Books.Include(b => b.Categories).ToList();
            var l2 = ctx.Categories.Include(c => c.Books).ToList();

            var ui_book_list = l1.Select(book =>
            {
                var c_list = book.Categories.Select(c => new SampleBookWC_Category()
                {
                    Id = c.Id,
                    Name = c.Name
                }).ToList();
                var b1 = new SampleBookWC()
                {
                    Id = book.Id,
                    Title = book.Title,
                    AuthorId = book.AuthorId,
                    ImageBase64 = book.ImageBase64,
                    PublishedDate = book.PublishedDate,
                    Categories = c_list
                };

                return b1;
            });

            var ui_cat_list = l2.Select(c => new
            {
                id = c.Id,
                name = c.Name,
                books = c.Books.Select(b => new
                {
                    id = b.Id,
                    Title = b.Title,
                    AuthorId = b.AuthorId,
                    ImageBase64 = b.ImageBase64,
                    PublishedDate = b.PublishedDate
                }).ToList()
            });

            return Ok(new
            {
                books = ui_book_list,
                categories = ui_cat_list
            });
        }

        [HttpGet("/api/users/info")]
        [Authorize]
        public IActionResult GetUserInfo()
        {
            var user = HttpContext.User;
            if (user?.Identity == null || !user.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            var userInfo = new
            {
                Name = user.Identity.Name,
                Claims = user.Claims.Select(c => new { c.Type, c.Value }).ToList(),
                username = user.Identity.Name,
            };

            return Ok(userInfo);
        }

        public class change_password_dto
        {
            public string username { get; set; }
            public string old_password { get; set; }
            public string new_password { get; set; }
        }

        [HttpPost("/api/users/changepassword")]
        [Authorize]
        public IActionResult ChangePassword([FromBody] change_password_dto in_obj)
        {
            var user_from_jwt = HttpContext.User;
            if (user_from_jwt?.Identity == null || !user_from_jwt.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }
            var username_from_jwt = user_from_jwt.Identity.Name;

            var user = ctx.Users.ToList().Where(u => u.Username.Equals(in_obj.username)).FirstOrDefault();
            if (user == null)
            {
                return NotFound(new
                {
                    message = "no user"
                });
            }

            if (!user.Username.Equals(username_from_jwt))
            {
                return Unauthorized();
            }

            if (user.Password.Equals(in_obj.old_password))
            {
                user.Password = in_obj.new_password;
                ctx.Users.Update(user);
                ctx.SaveChanges();

                return Ok(new
                {
                    message = "password changed"
                });
            }

            return BadRequest(new
            {
                message = "invalid credential"
            });
        }

        public static bool is_reset_token_still_valid(User? user)
        {

            if (user == null)
            {
                return false;
            }

            if (user.PasswordResetTokenExpiryUnixTS == null)
            {
                return false;
            }

            if (user.PasswordResetTokenExpiryUnixTS == null)
            {
                return false;
            }

            if (user.PasswordResetTokenExpiryUnixTS < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            {
                return false;
            }

            return true;
        }

        public class CheckPasswordResetTokenModel
        {
            public string token { get; set; }
        }

        [HttpPost("/api/users/check_password_reset_token")]
        public IActionResult CheckPasswordResetToken([FromBody] CheckPasswordResetTokenModel tokenModel)
        {
            try
            {
                if (tokenModel == null || string.IsNullOrEmpty(tokenModel.token))
                {
                    return BadRequest(new { message = "Invalid request data." });
                }

                string token = tokenModel.token;
                Console.WriteLine($"Checking password reset token: {token}");
                Console.WriteLine(token);
                if (string.IsNullOrEmpty(token))
                {
                    return BadRequest("Token is required.");
                }
                // Check if the token is valid
                var user = ctx.Users.FirstOrDefault(u => u.PasswordResetToken == token);
                if (user == null)
                {
                    return NotFound("Invalid or expired password reset token.");
                }

                bool is_valid = is_reset_token_still_valid(user);

                if (!is_valid)
                {
                    return NotFound("Invalid or expired password reset token.");
                }

                return Ok(new
                {
                    message = "Token is valid.",
                    user_id = user.Id,
                    username = user.Username,
                    email = user.Email
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "An error occurred while checking the password reset token.",
                    Details = ex.Message,
                    StackTrace = ex.StackTrace
                });
            }
        }

        public class ResetPasswordModel
        {
            public string token { get; set; }
            public string password { get; set; }
        }

        [HttpPost("/api/users/resetpassword")]
        public IActionResult ResetPassword([FromBody] ResetPasswordModel model)
        {
            if (model == null || string.IsNullOrEmpty(model.token) || string.IsNullOrEmpty(model.password))
            {
                return BadRequest(new { message = "Invalid request data." });
            }

            string token = model.token;
            string password = model.password;

            try
            {
                if (string.IsNullOrEmpty(password))
                {
                    return BadRequest("Password is required.");
                }
                // TODO Validate the password (e.g., length, complexity)
                if (string.IsNullOrEmpty(token))
                {
                    return BadRequest("Token is required.");
                }
                // Check if the token is valid
                var user = ctx.Users.FirstOrDefault(u => u.PasswordResetToken == token);
                if (user == null)
                {
                    return NotFound("Invalid or expired password reset token.");
                }

                bool is_valid = is_reset_token_still_valid(user);
                if (!is_valid)
                {
                    return NotFound("Invalid or expired password reset token.");
                }

                // Update the user's password
                user.Password = password; // In a real application, ensure to hash the password
                user.PasswordResetToken = null; // Clear the reset token
                user.PasswordResetTokenExpiryUnixTS = null; // Clear the expiry time
                ctx.SaveChanges();

                return Ok(new
                {
                    message = "Password changed!",
                    user_id = user.Id,
                    username = user.Username,
                    email = user.Email
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "An error occurred while resetting the password.",
                    Details = ex.Message,
                    StackTrace = ex.StackTrace
                });
            }
        }

        [HttpGet("/api/users/resetpassword")]
        public IActionResult CreateResetPasswordToken([FromQuery] string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    return BadRequest(new
                    {
                        messsage = "Email is required for password reset."
                    });
                }
                // Check if the user exists with the provided email
                var user = ctx.Users.FirstOrDefault(u => u.Email == email);
                if (user == null)
                {
                    return NotFound(new
                    {
                        message = "User with the provided email does not exist."
                    });
                }

                // Generate a password reset token
                var resetToken = Guid.NewGuid().ToString();
                user.PasswordResetToken = resetToken;
                user.PasswordResetTokenExpiryUnixTS = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds();
                ctx.SaveChanges();

                // Send the reset token via email
                var resetLink = $"{Request.Scheme}://{Request.Host}/resetpassword?token={resetToken}";
                var email_task = emailService.SendEmailAsync(
                    user.Email,
                    "Password Reset Request",
                    $"Please click the following link to reset your password: <a href=\"{resetLink}\">Reset Password</a><br><pre>{resetLink}</pre>"
                );

                email_task.Wait(); // Wait for the email to be sent

                return Ok(new { message = "Check your email for password reset link!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "An error occurred while resetting the password.",
                    Details = ex.Message,
                    StackTrace = ex.StackTrace
                });
            }
        }

        // TODO: Implement logout functionality
        // [HttpPost("/api/users/logout")]
        // [Authorize]
        // public IActionResult Logout()
        // {
        //     try
        //     {
        //         // Invalidate the user's session (this is a placeholder, actual implementation may vary)
        //         // For example, you might want to clear the authentication cookie or token
        //         HttpContext.SignOutAsync();
        //         return Ok(new { message = "User logged out successfully." });
        //     }
        //     catch (Exception ex)
        //     {
        //         return StatusCode(500, new
        //         {
        //             message = "An error occurred while logging out.",
        //             Details = ex.Message,
        //             StackTrace = ex.StackTrace
        //         });
        //     }
        // }

        public class UserRegistrationModel
        {
            public string username { get; set; }
            public string password { get; set; }
        }

        [HttpPost("/api/users/register")]
        [Authorize(Roles = "admin")]
        public IActionResult RegisterUser([FromBody] UserRegistrationModel model)
        {
            try
            {
                if (model == null || string.IsNullOrEmpty(model.username) || string.IsNullOrEmpty(model.password))
                {
                    return BadRequest("Invalid registration data.");
                }

                // Check if the user already exists
                var existingUser = ctx.Users.FirstOrDefault(u => u.Username == model.username);
                if (existingUser != null)
                {
                    return Conflict("User already exists.");
                }

                // Create a new user
                var newUser = new User
                {
                    Username = model.username,
                    Password = model.password, // In a real application, ensure to hash the password
                    Active = true,
                    Role = "user" // Default role, adjust as necessary
                };

                ctx.Users.Add(newUser);
                ctx.SaveChanges();

                return Ok(new
                {
                    message = "User registered successfully.",
                    id = newUser.Id,
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "An error occurred while registering the user.",
                    Details = ex.Message,
                    StackTrace = ex.StackTrace
                });
            }
        }
    }
}
