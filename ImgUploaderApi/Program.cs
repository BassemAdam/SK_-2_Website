using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<ImageUploadHandler>();
var app = builder.Build();


// Enable static files
app.UseStaticFiles();

app.MapPost("/upload", async (HttpContext context) =>
{
    var uploadHandler = app.Services.GetRequiredService<ImageUploadHandler>();
    var form = await context.Request.ReadFormAsync();
    var file = form.Files.GetFile("image");
    var title = form["title"];

    if (file != null && !string.IsNullOrEmpty(title))
    {
        var uploadedImage = await uploadHandler.UploadImageAsync(file, title);
        return Results.Json(new { id = uploadedImage.Id });
    }
    else
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        return Results.Text("No image uploaded or title provided.");
    }
}).Produces<JsonResult>();

app.MapGet("/imageStorage/{fileName}", async (string fileName, HttpContext context) =>
{
    var filePath = Path.Combine("imageStorage", fileName);
    if (File.Exists(filePath))
    {
        var fileStream = File.OpenRead(filePath);
        return Results.Stream(fileStream, "image/jpeg"); 
    }
    else
    {
        return Results.NotFound();
    }
});

app.MapGet("/picture/{id}", async (string id, HttpContext context) =>
{
    var uploadHandler = app.Services.GetRequiredService<ImageUploadHandler>();
    var uploadedImages = await uploadHandler.GetUploadedImagesAsync();
    var image = uploadedImages.FirstOrDefault(img => img.Id == id);
    //     UploadedImage image = null;
    // foreach (var img in uploadedImages)
    // {
    //     if (img.Id == id)
    //     {
    //         image = img;
    //         break;
    //     }
    // }

    if (image != null)
    {
        var fileInfo = new FileInfo(image.FilePath);
        var fileName = Path.GetFileName(fileInfo.Name);

        var html = $@"
            <html>
                <body>
                    <h1>{image.Title}</h1>
                    <img src='/imageStorage/{fileName}' alt='{image.Title}' /> <!-- Use the URL to the new endpoint -->
                </body>
            </html>";

        return Results.Content(html, "text/html");
    }
    else
    {
        return Results.NotFound();
    }
});

app.Run();

public class UploadedImage
{
    public string? Id { get; set; }
    public string? Title { get; set; }
    public string? FilePath { get; set; }
}

public class ImageUploadHandler
{
    private readonly string _imageStoragePath = "imageStorage";
    private readonly List<UploadedImage> _uploadedImages = new List<UploadedImage>();

    public ImageUploadHandler()
    {
        Directory.CreateDirectory(_imageStoragePath);
    }

    public async Task<UploadedImage> UploadImageAsync(IFormFile file, string title)
    {
        string id = Guid.NewGuid().ToString();
        string fileName = id + Path.GetExtension(file.FileName); // Include the file extension
        string filePath = Path.Combine(_imageStoragePath, fileName);
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var uploadedImage = new UploadedImage
        {
            Id = id,
            Title = title,
            FilePath = filePath
        };

        _uploadedImages.Add(uploadedImage);
        await SaveUploadedImagesToJsonAsync();

        return uploadedImage;
    }

    private async Task SaveUploadedImagesToJsonAsync()
    {
        string jsonFilePath = "uploadedImages.json";
        using (var stream = new StreamWriter(jsonFilePath))
        {
            await JsonSerializer.SerializeAsync(stream.BaseStream, _uploadedImages);
        }
    }

    public async Task<List<UploadedImage>> GetUploadedImagesAsync()
    {
        string jsonFilePath = "uploadedImages.json";
        if (File.Exists(jsonFilePath))
        {
            using (var stream = new StreamReader(jsonFilePath))
            {
                return await JsonSerializer.DeserializeAsync<List<UploadedImage>>(stream.BaseStream);
            }
        }
        else
        {
            return new List<UploadedImage>();
        }
    }
}