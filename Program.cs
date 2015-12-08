using System;

namespace ghazar_m
{
    internal class Program
    {
        // ReSharper disable once UnusedParameter.Local
        private static void Main(string[] args)
        {
			var path_of_bmp = "img/1.bmp";
			
			
			
            var pic = new ImageFilter(path_of_bmp);
			
			// WHAT TO DO ON THE IMAGE -- START
			
            pic.Edge1();
			
            pic.SaveImageTo("test.bmp");
			
            //ImageFilter.ReadBMPInfos("dragon.bmp"); //useless since bmp's data are seen from the constructor
			
			// END


            // ASCI-ART CREATOR -- START

			/*
            string txt;

            string ascii_name = "ASCII4.txt";

            pic.AsciiCreator(out txt);

            Console.Clear();
            Console.Write(txt);
            if (File.Exists(ascii_name)) File.Delete(ascii_name);
            using (var sw = new StreamWriter(ascii_name, true))
            {
                sw.Write(txt);
            }
			*/

            // END

			
			// EXAMPLES OF THINGS TO DO (do NOT use 'ImageFiler.something', use methodes of the 'ImageFilter' instance, like the 'pic' object)
			
            /*
            ImageFilter.Sharpen();
            ImageFilter.SaveImageTo("sharpen.bmp");ImageFilter.LoadImageFrom("wolf.bmp"); // reload image except if you want several filters at the same time
																						// OR use the methode 'Undo' (experimental)
            ImageFilter.Edge1();
            ImageFilter.SaveImageTo("edge1.bmp"); ImageFilter.LoadImageFrom("wolf.bmp");

            ImageFilter.Edge2();
            ImageFilter.SaveImageTo("edge2"); ImageFilter.LoadImageFrom("wolf.bmp");

            ImageFilter.Edge3();
            ImageFilter.SaveImageTo("edge3.bmp"); ImageFilter.LoadImageFrom("wolf.bmp");

            ImageFilter.Blur();
            ImageFilter.SaveImageTo("blur.bmp"); ImageFilter.LoadImageFrom("wolf.bmp");

            ImageFilter.Gaussian();
            ImageFilter.SaveImageTo("gaussian.bmp"); ImageFilter.LoadImageFrom("wolf.bmp");

            ImageFilter.Identity();
            ImageFilter.SaveImageTo("identity.bmp"); ImageFilter.LoadImageFrom("wolf.bmp");
            */
			
			Console.WriteLine("END");
            Console.ReadLine();
        }
    }
}