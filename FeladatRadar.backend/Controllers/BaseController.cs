using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FeladatRadar.backend.Controllers
{
    /// <summary>
    /// Alap controller osztály: közös JWT segédmetódusok minden controllerhez.
    /// </summary>
    public abstract class BaseController : ControllerBase
    {
        /// <summary>
        /// Visszaadja a bejelentkezett felhasználó ID-ját a JWT tokenből.
        /// </summary>
        /// <exception cref="UnauthorizedAccessException">Ha a token érvénytelen.</exception>
        protected int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(claim) || !int.TryParse(claim, out int userId))
                throw new UnauthorizedAccessException("Érvénytelen token.");
            return userId;
        }

        /// <summary>
        /// Visszaadja a bejelentkezett felhasználó email-címét.
        /// </summary>
        protected string GetCurrentUserEmail()
            => User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;

        /// <summary>
        /// Visszaadja a bejelentkezett felhasználó szerepkörét.
        /// </summary>
        protected string GetCurrentUserRole()
            => User.FindFirst(ClaimTypes.Role)?.Value ?? "Student";
    }
}
