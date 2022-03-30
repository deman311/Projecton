using Microsoft.AspNetCore.Mvc;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Amazon.MachineLearning.Model;

namespace aspnetcore.Controllers
{
    [ApiController]
    [Route("/catordog")]
    public class InferenceController : ControllerBase
    {
        private InferenceSession _session;

        public InferenceController(InferenceSession session)
        {
            _session = session;
        }

        [HttpPost]
        public ActionResult Score()
        {
            // load a test image
            var image = Image.Load<Rgb24>("./jim.jpg");
            //var image = Image.Load<Rgb24>("./catos.jpg");
            image = image.Clone(ctx =>
            {
                ctx.Resize(new ResizeOptions
                {
                    Size = new Size(224, 224),
                    Mode = ResizeMode.Stretch
                });
            });

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
            var prediction = new Prediction { PredictedLabel = GetLabel(ARG_Softmax(probs)) };

            result.Dispose(); // free resources
            return Ok(prediction); // return marshalled prediction
        }

        // decode label
        private string GetLabel(int value)
        {
            switch (value)
            {
                case 0: return "Cat";
                case 1: return "Dog";
                default: return "Other";
            }
        }

        // softmax then argmax
        private int ARG_Softmax(float[] z)
        {
            for (int i = 0; i < z.Length; i++)
                z[i] = (float)Math.Exp(z[i]);
            var sum_z_exp = z.Sum();
            var softmax = z.Select(i => i / sum_z_exp);

            float max_val = softmax.Max();
            return softmax.ToList().IndexOf(max_val);
        }
    }
}