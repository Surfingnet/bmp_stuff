using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace ghazar_m
{
    internal class ImageFilter
    {
        private static readonly char[] Ascii = { '@', '#', '8', '&', '+', 'o', ':', '*', '.', ' ' };//density ASCII array

        //more attributes
        private static byte[] _header;
        private Bitmap[] _bitmaps;
        private Bitmap _data;
        private int _height;
        private int _rangActuel;
        private int _width;

        public ImageFilter()//constructor
        {
            _bitmaps = new Bitmap[0];
            _rangActuel = -1;
        }

        public ImageFilter(string filename)//constructor overload
        {
            _bitmaps = new Bitmap[0];
            _rangActuel = -1;
            LoadImageFrom(filename);
        }

        private bool is_loaded()//kind of explicit
        {
            if (_data != null) return true;
            Console.Error.WriteLine("> No image loaded.");
            Console.ReadLine();
            return false;
        }

        /// <summary>
        ///     if the file is a bitmap, move stream reader position forward by 2 bytes and continue by returning 'true'. if not, it stops after returning 'false'.
        /// </summary>
        /// <param name="fs"></param>
        /// <returns>bitmap or not</returns>
        private static bool is_bitmap(Stream fs)//read file header's magical numbers (to see if it s a bmp, undepending on the file extension)
        {
            fs.Seek(0, SeekOrigin.Begin);
            if (fs.Length < 2) return false;
            int magic1 = fs.ReadByte();
            int magic2 = fs.ReadByte();
            return magic1 == 0x42 && magic2 == 0x4d;
        }

        private void set_private(int width, int height)//set privates attributes
        {
            if (_data == null) return;
            _width = width;
            _height = height;
        }

        // ReSharper disable once InconsistentNaming
        public static void ReadBMPInfos(string filename)//for fun, read and gives image's infos"
        {
            Console.Clear();
            try
            {
                if (!File.Exists(filename)) //check if file exists
                {
                    Console.Error.WriteLine("> The file does not exist.");
                    Console.ReadLine();
                    return;
                }
                using (var filestream = new FileStream(filename, FileMode.Open, FileAccess.ReadWrite))//open streamreader
                {
                    if (!is_bitmap(filestream))//check type
                    {
                        Console.Error.WriteLine("> the file is not a Bitmap.");
                        Console.ReadLine();
                        return;
                    }

                    _header = new byte[54];

                    filestream.Seek(0, SeekOrigin.Begin);//move stream reader position at start point, just in case.

                    for (int i = 0x0; i < 0x36; i++)//take header's meta-datas
                        _header[i] = (byte)filestream.ReadByte();

                    Console.Clear();
						
					//collect dimensions and bit per pixel, depend on the bmp's header architecture. If it's not good, it won't work.
					
                    int width =
                        Convert.ToInt32(_header[18] + _header[19] * 256 + _header[20] * 65536 + _header[21] * 4294967296);
                    int height =
                        Convert.ToInt32(_header[22] + _header[23] * 256 + _header[24] * 65535 + _header[25] * 4294967296);
                    byte bitsPerPixel = _header[28];

                    Console.WriteLine("> Width           : " + width + " pixels");
                    Console.WriteLine("> Height          : " + height + " pixels");
                    Console.WriteLine("> Bits per pixels : " + bitsPerPixel + " bits");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The process failed: {0}", e);
            }
            Console.ReadLine();
        }

        public bool LoadImageFrom(string filename)//load img from a specific path in the memory
        {
            Console.Clear();
            try
            {
                if (!File.Exists(filename))//check if the file exists
                {
                    Console.Error.WriteLine("> The file does not exist.");
                    Console.ReadLine();
                    return false;
                }

                using (var fs = new FileStream(filename, FileMode.Open, FileAccess.ReadWrite))
                {
                    if (!is_bitmap(fs))//check type
                    {
                        Console.Error.WriteLine("> the file is not a Bitmap.");
                        Console.ReadLine();
                        return false;
                    }
                    //fs.Seek(0, SeekOrigin.Begin);
                    _data = new Bitmap(fs);
                    set_private(_data.Width, _data.Height);
                }

                Add(_data);

                Console.WriteLine("> {0} successfully loaded.", filename);

                Console.ReadLine();

                return true;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("> An error occured : \n {0}", e);
                Console.ReadLine();
                return false;
            }
        }

        public bool SaveImageTo(string filename)
        {
            try
            {
                if (_data != null)
                {
                    if (File.Exists(filename))
                    {
                        Console.WriteLine("> {0} already exists. Overwrite ? (Y/N)", filename);
                        string tmp = Console.ReadLine();
                        if (tmp == "y" || tmp == "Y")
                        {
                            File.Delete(filename);
                        }
                        else
                        {
                            Console.WriteLine("> Enter a custom name without the file extension. (empty to abort)");
                            filename = Console.ReadLine();
                            if (filename == "")
                            {
                                Console.Error.WriteLine("> Aborted.");
                                Console.ReadLine();
                                return false;
                            }
                            filename += ".bmp";
                        }
                    }

                    if (string.IsNullOrEmpty(filename))
                    {
                        Console.Error.WriteLine("> Bad name");
                        return false;
                    }

                    SavePicture(_data, filename);

                    Console.WriteLine("> {0} successfully saved.", filename);
                    Console.ReadLine();
                    return true;
                }
                Console.Error.WriteLine("> No image to save");
                Console.ReadLine();
                return false;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("> An error occured : \n {0}", e);
                Console.ReadLine();
                return false;
            }
        }

        private static void SavePicture(Bitmap entry, string filename)
        //Sauver dans la fonction principale me fait une erreur, avec string ou stream. 
        //Ici ca fonctionne
        {
            var bm = new Bitmap(entry);
            bm.Save(filename, ImageFormat.Bmp);
        }

        public void AsciiCreator(out string output)
        {
            output = "";

            if (!is_loaded())
                return;

            if (_width > 80)
            {
                Console.WriteLine(
                    "> The image dimensions might not fit the console size\n> Reduce it ? (Y/N)\n> (it can REALLY save a lot of time, but there is an evident loss of quality)");
                string yes = Console.ReadLine();
                if (yes == "y" || yes == "Y")
                {
                    //pour un affichage completement useless...
                    int count = 1;
                    int anotherTmp = _data.Width;
                    int to = 0;
                    while (anotherTmp > 80)
                    {
                        anotherTmp /= 2;
                        to++;
                    }
                    Console.Clear();
                    while (_data.Width >= 80)
                    {
                        Console.Clear();
                        Console.WriteLine("> Reducing... {0} of {1}", count, to);
                        Reduce();
                        count++;
                    }
                    Console.WriteLine(
                        "> Successfully reduced.\n> If you want the original image back, you need to reload it.");
                    Console.ReadLine();
                }

                else
                {
                    Console.WriteLine("> Continue with this image anyway ? (Y/N)");
                    yes = Console.ReadLine();
                    if (yes != "y" && yes != "Y")
                    {
                        Console.WriteLine("> Aborted.");
                        Console.ReadLine();
                        return;
                    }
                }
            }

            Console.WriteLine("> Greyscaling the image...");
            GrayScale();
            Console.WriteLine(
                "> The image is now in greyscale.\n> If you want the original image back, you need to reload it.");
            Console.ReadLine();

            Bitmap tmp = _data;
            int pxlnb = _width * _height;

            for (int i = 0; i < _height; i++)
            {
                Console.Clear();
                Console.Write("> Pixels to convert : {0}", pxlnb);

                for (int j = 0; j < _width; j++)
                {
                    Color color = tmp.GetPixel(j, i);
                    int process = Convert.ToInt32(color.R / 255.0 * 8.0);

                    if (process < 0) process = process * (-1);

                    output += Ascii[process];

                    pxlnb--;
                }
                //if (i == _height) continue;
                output += "\n";
            }
            Console.Clear();
            Console.WriteLine(output);
            Console.ReadLine();
        }

        public void GrayScale()
        {
            if (!is_loaded())
                return;

            var bmp = new Bitmap(_width, _height);
            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color pixelColor = _data.GetPixel(j, i);
                    int middle = Convert.ToInt32(((pixelColor.R + pixelColor.G + pixelColor.B) / 3.0) * (pixelColor.A / 255.0));
                    //grace a middle, cela fonctionne avec n'importe quelle couleur
                    // à n'importe quel niveau d'alpha de 0 a 255, on ne perd pas l'effet de transparence.
                    Color newColor = Color.FromArgb(255, middle, middle, middle);
                    bmp.SetPixel(j, i, newColor);
                }
            }
            Add(bmp);
            _data = bmp;
            Console.WriteLine("> Image now in grayscale.");
            Console.ReadLine();
        }

        public void Reduce()
        {
            if (!is_loaded())
                return;

            var red = new Bitmap(_data.Width / 2, _data.Height / 2, _data.PixelFormat);
            for (int i = 0; i < red.Height; i++)
            {
                for (int j = 0; j < red.Width; j++)
                {
                    Color pixelColor1 = _getPixel(j * 2, i * 2);
                    Color pixelColor2 = _getPixel(j * 2 + 1, i * 2);
                    Color pixelColor3 = _getPixel(j * 2, i * 2 + 1);
                    Color pixelColor4 = _getPixel(j * 2 + 1, i * 2 + 1);

                    int pixelA = (pixelColor1.A + pixelColor2.A + pixelColor3.A + pixelColor4.A) / 4;
                    int pixelR = (pixelColor1.R + pixelColor2.R + pixelColor3.R + pixelColor4.R) / 4;
                    int pixelG = (pixelColor1.G + pixelColor2.G + pixelColor3.G + pixelColor4.G) / 4;
                    int pixelB = (pixelColor1.B + pixelColor2.B + pixelColor3.B + pixelColor4.B) / 4;

                    Color newColor = Color.FromArgb(pixelA, pixelR, pixelG, pixelB);

                    red.SetPixel(j, i, newColor);
                }
            }
            Add(red);
            _data = red;
            _width = red.Width;
            _height = red.Height;
        }

        /// <summary>
        ///     Gives color of pixel at coords x,y.
        ///     Can handle out of range pixels, essential for every kernel use, and the img reduction.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>color</returns>
        private Color _getPixel(int x, int y)
        {
            if (x < 0) x = 0;
            else if (x >= _width) x = _width - 1;
            if (y < 0) y = 0;
            else if (y >= _height) y = _height - 1;
            return _data.GetPixel(x, y);
        }

        private Bitmap process_image_kernel(int[,] kernel)//apply a standard 3*3 kernel on every pixels of the image)
        {
            var bmp = new Bitmap(_width, _height);//create a new bmp with same dimensions as the current one
            int total = _width * _height;

            Console.Clear();
            for (int j = 0; j < _height; j++)
            {
                for (int i = 0; i < _width; i++)
                {
                    total--;
                    bmp.SetPixel(i, j, process_pixel_kernel(kernel, i, j));//take the pixel (at i,j coords) from current bmp, and saving the new pixel on the new bmp.
                }
                Console.Clear();
                Console.WriteLine("> Processing...\n> Pixels left : {0}\n", total);
            }
            Console.Clear();
            Console.WriteLine("> Finished.");
            return bmp;
        }

        private Color process_pixel_kernel(int[,] kernel, int x, int y)
        {
            int sum = kernel[0, 0] + kernel[0, 1] + kernel[0, 2] + kernel[1, 0] + kernel[1, 1] + kernel[1, 2] +
                      kernel[2, 0] + kernel[2, 1] + kernel[2, 2];

            int r = process_color_kernel(kernel, sum, x, y, 'r');
            int g = process_color_kernel(kernel, sum, x, y, 'g');
            int b = process_color_kernel(kernel, sum, x, y, 'b');

            return Color.FromArgb(255, r, g, b);
        }

        private int process_color_kernel(int[,] kernel, int sum, int x, int y, char rgb)
        {
            int accumulator = 0;

            if (sum == 0) sum = 1;

            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    if (kernel[i + 1, j + 1] == 0) continue;
                    switch (rgb)
                    {
                        case 'r':
                            accumulator += _getPixel(x + i, y + j).R * kernel[i + 1, j + 1] / sum;
                            break;
                        case 'g':
                            accumulator += _getPixel(x + i, y + j).G * kernel[i + 1, j + 1] / sum;
                            break;
                        case 'b':
                            accumulator += _getPixel(x + i, y + j).B * kernel[i + 1, j + 1] / sum;
                            break;
                        default:
                            throw new Exception("Bad color. Use 'R', 'G' or 'B'.");
                    }
                }
            }
            int res = accumulator;
            //if (sum != 0)
            // res = (Math.Abs(accumulator) / Math.Abs(sum));
            if (res > 255)
                res = 255;
            if (res < 0)
                res = 0;
            return res;
        }

		//some standards kernels
		
        public void Identity()
        {
            if (!is_loaded())
                return;

            int[,] kernel = { { 0, 0, 0 }, { 0, 1, 0 }, { 0, 0, 0 } };
            Bitmap bmp = process_image_kernel(kernel);
            Add(bmp);
            _data = bmp;
        }

        public void Edge1()
        {
            if (!is_loaded())
                return;

            int[,] kernel = { { 1, 0, -1 }, { 0, 0, 0 }, { -1, 0, 1 } };
            Bitmap bmp = process_image_kernel(kernel);
            Add(bmp);
            _data = bmp;
        }

        public void Edge2()
        {
            if (!is_loaded())
                return;

            int[,] kernel = { { 0, 1, 0 }, { 1, -4, 1 }, { 0, 1, 0 } };
            Bitmap bmp = process_image_kernel(kernel);
            Add(bmp);
            _data = bmp;
        }

        public void Edge3()
        {
            if (!is_loaded())
                return;

            int[,] kernel = { { -1, -1, -1 }, { -1, 8, -1 }, { -1, -1, -1 } };
            Bitmap bmp = process_image_kernel(kernel);
            Add(bmp);
            _data = bmp;
        }

        public void Sharpen()
        {
            if (!is_loaded())
                return;

            int[,] kernel = { { 0, -1, 0 }, { -1, 5, -1 }, { 0, -1, 0 } };
            Bitmap bmp = process_image_kernel(kernel);
            Add(bmp);
            _data = bmp;
        }

        public void Blur()
        {
            if (!is_loaded())
                return;

            int[,] kernel = { { 1, 1, 1 }, { 1, 1, 1 }, { 1, 1, 1 } };
            Bitmap bmp = process_image_kernel(kernel);
            Add(bmp);
            _data = bmp;
        }

        public void Gaussian()
        {
            if (!is_loaded())
                return;

            int[,] kernel = { { 1, 2, 1 }, { 2, 4, 2 }, { 1, 2, 1 } };
            Bitmap bmp = process_image_kernel(kernel);
            Add(bmp);
            _data = bmp;
        }

        public void Unsharp()
        {
            if (!is_loaded())
                return;

            int[,] kernel =
            {
                {1, 4, 6, 4, 1}, {4, 16, 24, 16, 4}, {6, 24, -476, 24, 6}, {4, 16, 24, 16, 4},
                {1, 4, 6, 4, 1}
            };
            Bitmap bmp = process_image_custom_kernel(kernel, 5, 5, 0);
            Add(bmp);
            _data = bmp;
        }

        public bool Undo()//previous version of bmp
        {
            if (!is_loaded())
                return false;

            if (_rangActuel < 1)
            {
                Console.Error.WriteLine("> You can't do this !");
                Console.ReadLine();
                return false;
            }
            _rangActuel -= 1;
            _data = _bitmaps[_rangActuel];
            _height = _data.Height;
            _width = _data.Width;
            Console.WriteLine("> Undone.");
            Console.ReadLine();
            return true;
        }

        public bool Redo()//next version of bmp
        {
            if (!is_loaded())
                return false;

            if (_rangActuel == _bitmaps.Length - 1)
            {
                Console.Error.WriteLine("> You can't do this !");
                Console.ReadLine();
                return false;
            }

            _rangActuel += 1;
            _data = _bitmaps[_rangActuel];
            _height = _data.Height;
            _width = _data.Width;
            Console.WriteLine("> Redone.");
            Console.ReadLine();
            return true;
        }

        private void Add(Bitmap bmp)//prototype of versioning system
        {
            var tmp = new Bitmap[_rangActuel + 2];
            for (int i = 0; i <= _rangActuel; i++)
                tmp[i] = _bitmaps[i];
            tmp[_rangActuel + 1] = bmp;
            _rangActuel += 1;
            _bitmaps = tmp;
        }

        private int process_color_custom_kernel(int[,] kernel, int kernelX, int kernelY, int bias, int sum, int x,
            int y,
            char rgb)//Apply a custom kernel of ANY size (it needs odd dimensions, it needs a center)
        {
            int accumulator = 0;

            if (sum == 0) sum = 1;

            for (int i = -(kernelX / 2); i < kernelX / 2; i++)
            {
                for (int j = -(kernelY / 2); j < kernelY / 2; j++)
                {
                    if (kernel[i + (kernelX / 2), j + (kernelY / 2)] == 0) continue;
                    switch (rgb)
                    {
                        case 'r':
                            accumulator += _getPixel(x + i, y + j).R * kernel[i + (kernelX / 2), j + (kernelY / 2)] / sum;
                            break;
                        case 'g':
                            accumulator += _getPixel(x + i, y + j).G * kernel[i + (kernelX / 2), j + (kernelY / 2)] / sum;
                            break;
                        case 'b':
                            accumulator += _getPixel(x + i, y + j).B * kernel[i + (kernelX / 2), j + (kernelY / 2)] / sum;
                            break;
                        default:
                            throw new Exception("Bad color. Use 'R', 'G' or 'B'.");
                    }
                }
            }
            int res = accumulator + bias;
            // if (sum != 0)
            //   res = (Math.Abs(accumulator) / sum);
            if (res > 255)
                res = 255;
            if (res < 0)
                res = 0;
            return res;
        }

        private Color process_pixel_custom_kernel(int[,] kernel, int kernelX, int kernelY, int bias, int x, int y)//process a pixel
        {
            int sum = 0;

            for (int i = 0; i < kernelX; i++)
            {
                for (int j = 0; j < kernelY; j++)
                {
                    sum += kernel[i, j];
                }
            }

            int r = process_color_custom_kernel(kernel, kernelX, kernelY, bias, sum, x, y, 'r');
            int g = process_color_custom_kernel(kernel, kernelX, kernelY, bias, sum, x, y, 'g');
            int b = process_color_custom_kernel(kernel, kernelX, kernelY, bias, sum, x, y, 'b');

            return Color.FromArgb(255, r, g, b);
        }

        private Bitmap process_image_custom_kernel(int[,] kernel, int kernelX, int kernelY, int bias)//process the image
        {
            var bmp = new Bitmap(_width, _height);
            int total = _width * _height;

            Console.Clear();
            for (int j = 0; j < _height; j++)
            {
                for (int i = 0; i < _width; i++)
                {
                    total--;
                    bmp.SetPixel(i, j, process_pixel_custom_kernel(kernel, kernelX, kernelY, bias, i, j));
                }
                Console.Clear();
                Console.WriteLine("> Processing...\n> Pixels left : {0}\n", total);
            }
            return bmp;
        }


        //to enter your kernel in the console !
        public void start_custom_kernel()
        {
            if (!is_loaded())
                return;

            int kernelX = 0;
            int kernelY = 0;
            int bias = 0;
            int[,] kernel = { };
            bool everythingSFind = false;
            bool stopProcess = false;

            while (!everythingSFind)
            {
                Console.Clear();
                bool ok = false;
                while (!ok)
                {
                    Console.Clear();
                    Console.WriteLine("> Enter your kernel's width (odd int > 2)");
                    ok = Int32.TryParse(Console.ReadLine(), out kernelX) && kernelX % 2 != 0 && kernelX > 2;
                }

                ok = false;
                while (!ok)
                {
                    Console.Clear();
                    Console.WriteLine("> Enter your kernel's height (odd int > 2)");
                    ok = Int32.TryParse(Console.ReadLine(), out kernelY) && kernelY % 2 != 0 && kernelY > 2;
                }

                kernel = new int[kernelX, kernelY];

                ok = false;
                while (!ok)
                {
                    for (int x = 0; x < kernelX; x++)
                        for (int y = 0; y < kernelY; y++)
                        {
                            bool ok2 = false;
                            while (!ok2)
                            {
                                Console.Clear();
                                Console.WriteLine("> Enter your kernel's value at coords [{0},{1}]", x, y);
                                ok2 = Int32.TryParse(Console.ReadLine(), out kernel[x, y]);
                            }
                        }
                    Console.Clear();
                    Console.WriteLine("> Here is your kernel :\n");

                    for (int x = 0; x < kernelX; x++)
                    {
                        for (int y = 0; y < kernelY; y++)
                        {
                            Console.Write(" {0} ", kernel[x, y]);
                        }
                        Console.Write("\n");
                    }
                    Console.WriteLine("\n> Add a Bias to your filter ? (Y/N)\n");
                    string yesNo4 = Console.ReadLine();
                    if (yesNo4 != "Y" && yesNo4 != "y")
                    {
                        bias = 0;
                    }
                    else
                    {
                        bool ok3 = false;
                        while (!ok3)
                        {
                            Console.Clear();
                            Console.WriteLine(
                                "> Enter a bias value.\n> (any relative integer, not farther from 0 than 255 or -255, be rational....)");
                            ok3 = Int32.TryParse(Console.ReadLine(), out bias);
                        }
                    }
                    Console.Clear();
                    Console.WriteLine("> Here is your kernel :\n");

                    for (int x = 0; x < kernelX; x++)
                    {
                        for (int y = 0; y < kernelY; y++)
                        {
                            Console.Write(" {0} ", kernel[x, y]);
                        }
                        Console.Write("\n");
                    }
                    Console.Write("\n> Bias = {0}\n\n", bias);
                    Console.WriteLine("> Apply this kernel to the image ? (Y/N)");
                    string yesNo = Console.ReadLine();
                    if (yesNo != "Y" && yesNo != "y")
                    {
                        Console.Clear();
                        Console.WriteLine("> Enter a new kernel ?");
                        string yesNo2 = Console.ReadLine();
                        if (yesNo2 != "Y" && yesNo2 != "y")
                        {
                            everythingSFind = true;
                            ok = true;
                            stopProcess = true;
                        }
                        else
                        {
                            ok = true;
                        }
                    }
                    else
                    {
                        everythingSFind = true;
                        ok = true;
                    }
                }
            }

            if (!stopProcess)
            {
                Bitmap bmp = process_image_custom_kernel(kernel, kernelX, kernelY, bias);
                Add(bmp);
                _data = bmp;
                Console.Clear();
                Console.WriteLine("> Finished");
            }
            else
            {
                Console.Clear();
                Console.Error.WriteLine("> Aborted.");
            }
        }

        //3d wowowow
        public void Emboss()
        {
            if (!is_loaded())
                return;

            int[,] kernel = { { -2, -1, 0 }, { -1, 1, 1 }, { 0, 1, 2 } };

            Bitmap bmp = process_image_kernel(kernel);
            Add(bmp);
            _data = bmp;
        }

        //some bigger kernels
        public void Dilatation()
        {
            if (!is_loaded())
                return;

            int[,] kernel =
            {
                {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
                {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                {1, 0, 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 0, 0, 1},
                {1, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 1},
                {1, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 1},
                {1, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 1},
                {1, 0, 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 0, 0, 1},
                {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1}
            };
            Bitmap bmp = process_image_custom_kernel(kernel, 15, 15, 50);
            Add(bmp);
            _data = bmp;
        }

        //erosion
        public void Erosion()
        {
            if (!is_loaded())
                return;

            int[,] kernel =
            {
                {0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0},
                {0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0},
                {0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0},
                {0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0},
                {1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1},
                {0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0},
                {0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0},
                {0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0},
                {0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0},
                {0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0},
                {1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1},
                {0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0},
                {0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0},
                {0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0},
                {0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0}
            };
            Bitmap bmp = process_image_custom_kernel(kernel, 15, 15, 35);
            Add(bmp);
            _data = bmp;
        }

        //another blur, change the contrast.
        public void Deamon()
        {
            if (!is_loaded())
                return;

            int[,] kernel =
            {
                {0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0},
                {0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0},
                {0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0},
                {0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0},
                {0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0},
                {0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0},
                {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
                {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
                {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
                {0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0},
                {0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0},
                {0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0},
                {0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0},
                {0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0},
                {0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0}
            };
            Bitmap bmp = process_image_custom_kernel(kernel, 15, 15, 50);
            Add(bmp);
            _data = bmp;
        }

        //more aggresive sharpening.
        public void Sharpen2()
        {
            if (!is_loaded())
                return;

            int[,] kernel = { { -1, -1, -1 }, { -1, 9, -1 }, { -1, -1, -1 } };

            Bitmap bmp = process_image_kernel(kernel);
            Add(bmp);
            _data = bmp;
        }

        /// <summary>
        ///     set opacity to the max and keep good colors depending on the past transparency
        /// </summary>
        /// <returns></returns>
        // ReSharper disable once UnusedMember.Local
        public void Without_transparency()
        {
            if (!is_loaded())
                return;

            var bmp = new Bitmap(_width, _height);

            for (int j = 0; j < _height; j++)
            {
                for (int i = 0; i < _width; i++)
                {
                    byte transperency = _data.GetPixel(i, j).A;
                    double fact = Convert.ToDouble(transperency) / 255.0;
                    bmp.SetPixel(i, j,
                        (Color.FromArgb(255, Convert.ToInt32(Convert.ToDouble(_data.GetPixel(i, j).R) * fact),
                            Convert.ToInt32(Convert.ToDouble(_data.GetPixel(i, j).G) * fact),
                            Convert.ToInt32(Convert.ToDouble(_data.GetPixel(i, j).B) * fact))));
                }
            }
            Add(bmp);
            _data = bmp;
        }
    }
}