using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Scraper.Contract.Services;

namespace Scraper.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShowsController : ControllerBase
    {
        private readonly IShowService showService;

        public ShowsController(IShowService showService)
        {
            this.showService = showService;
        }

        // GET api/shows/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            return Ok(await showService.GetAsync(id));
        }

        [HttpGet("pageId={pageId}")]
        public async Task<IActionResult> GetShowPage(int pageId)
        {
            var result = await showService.GetShowPageAsync(pageId);
            foreach(var show in result)
            {
                show.Cast = show.Cast.OrderByDescending(actor => actor.Birthday);
            }
            return Ok(result);
        }
    }
}
