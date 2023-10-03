using System.Net;
using Microsoft.AspNetCore.Mvc;
using OpenQA.Selenium.DevTools;
using Streamer.Exceptions;

namespace Streamer.Controllers;

[ApiController]
[Route("api/developers")]
public class DevelopersController : ControllerBase
{
    private readonly IWebHostEnvironment _env;
    public DevelopersController(IWebHostEnvironment env)
    {
        _env = env;
    }
    
    [HttpPost]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        string fileExtension = Path.GetExtension(file.FileName).ToLower();
        if (!fileExtension.Equals(".html")) throw new IncorrectFileType();
        
        if (System.IO.File.Exists(_env.WebRootPath + $"/{file.FileName}")) throw new FileAlreadyExists();
        
        using (FileStream stream = new FileStream(_env.WebRootPath + $"/{file.FileName}", FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return Ok();
    }

    [HttpGet("{fileName}")]
    public async Task GetScript(string fileName)
    {
        string script = (await System.IO.File.ReadAllTextAsync(_env.WebRootPath + "/sdk.js"))
            .Replace("@{CustomFileName}", fileName);
        HttpContext.Response.ContentType = "text/javascript";
        HttpContext.Response.StatusCode = 200;
        await HttpContext.Response.WriteAsync(script);
    }
}