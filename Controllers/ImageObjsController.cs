#nullable disable
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

/**
 * This section is a auto-generated API controller for a DB.
 * Scaffold Item -> API controller with actions using the Entity Framework
 * (The POST method is written by me)
 */
namespace Projecton.Controllers
{
    [Route("api/CatOrDogDB")]
    [ApiController]
    public class ImageObjsController : ControllerBase
    {
        static int counter = 1; // counter for the UID in the database

        private readonly ImageContext _context; // for the DataBase
        private InferenceSession _session;      // for the ONNX model

        public ImageObjsController(ImageContext context, InferenceSession session)
        {
            _context = context;
            _session = session;
        }

        // GET: api/CatOrDogDB
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ImageObj>>> GetImageItems()
        {
            return await _context.Images.ToListAsync();
        }

        // GET: api/CatOrDogDB/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ImageObj>> GetImageObj(int id)
        {
            var imageObj = await _context.Images.FindAsync(id);

            if (imageObj == null)
            {
                return NotFound();
            }

            return imageObj;
        }

        // PUT: api/CatOrDogDB/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutImageObj(int id, ImageObj imageObj)
        {
            if (id != imageObj.Id)
            {
                return BadRequest();
            }

            _context.Entry(imageObj).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ImageObjExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/CatOrDogDB
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("{file}")]
        public async Task<ActionResult<ImageObj>> PostImageObj([FromForm] IFormFile file)
        {
            // Read the image from the passed file to memory
            var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            memoryStream.Position = 0; // reset seek pointer to head of file for loading

            // load image to variable from memory
            var image = Image.Load<Rgb24>(memoryStream);
            if (image == null)
                return BadRequest("Problem with reading the image.");

            image = image.Clone(ctx => {
                ctx.Resize(new ResizeOptions {
                    Size = new Size(224, 224),
                    Mode = ResizeMode.Stretch
                });
            });
            memoryStream.Close();

            // convert image to input format
            var input = new DenseTensor<float>(new[] { 1, 3, 224, 224 });
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    input[0, 0, y, x] = image[x, y].R;
                    input[0, 1, y, x] = image[x, y].G;
                    input[0, 2, y, x] = image[x, y].B;
                }
            }

            // inject the image into the model
            var result = _session.Run(new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("IN", input)
            });

            // check if output is valid - as expected
            if (result.FirstOrDefault()?.Value is not Tensor<float> output)
                throw new ApplicationException("Unable to process image");

            // convert into probabilities and decode to label
            float[] probs = result.First().AsTensor<float>().ToArray();
            var prediction = GetLabel(ARG_Softmax(probs));

            result.Dispose(); // free resources

            // Convert the image into the 224x224 format to input into the ImageObj
            MemoryStream ms = new MemoryStream(); // using memoryStream to convert into byte[]
            image.SaveAsPng(ms);

            ImageObj imageObj = new ImageObj();
            imageObj.Id = counter++;
            imageObj.Tag = prediction;
            imageObj.Data = ms.ToArray();
            ms.Close();

            // Save to DB
            _context.Images.Add(imageObj);
            await _context.SaveChangesAsync();

            return Ok(prediction); // return 200 OK with the prediction label
        }

        // DELETE: api/CatOrDogDB/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteImageObj(int id)
        {
            var imageObj = await _context.Images.FindAsync(id);
            if (imageObj == null)
            {
                return NotFound();
            }

            _context.Images.Remove(imageObj);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ImageObjExists(int id)
        {
            return _context.Images.Any(e => e.Id == id);
        }

        // decode label
        private string GetLabel(int value) {
            switch (value)
            {
                case 0: return "Cat";
                case 1: return "Dog";
                default: return "Other";
            }
        }

        // softmax then argmax
        private int ARG_Softmax(float[] z) {
            for (int i = 0; i < z.Length; i++)
                z[i] = (float)Math.Exp(z[i]);
            var sum_z_exp = z.Sum();
            var softmax = z.Select(i => i / sum_z_exp);

            float max_val = softmax.Max();
            return softmax.ToList().IndexOf(max_val);
        }
    }
}
