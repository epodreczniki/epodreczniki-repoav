using System;
using System.Collections.Generic;
using System.Linq;
using PSNC.Multimedia;
using PSNC.Multimedia.Tools;

namespace MediaParserTest
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                return;
            }
            string path = args[0];

            try
            {
                Console.WriteLine(path.ToString());
                using (MediaParser parser = new MediaParser(path))
                {
                    Console.WriteLine("typ kompilacji: " + parser.GetCompilationType());
                    Console.WriteLine("użycie dodatkowych bibliotek: " + (parser.UsesAdditionalLibraries() ? "Tak" : "Nie"));
                    var start = DateTime.Now;
                    IMediaParserInstance instance = parser.Parse();
                    var stop = DateTime.Now;
                    Console.WriteLine(String.Format("Parsowanie trwało {0} milisekund", (stop - start).TotalMilliseconds));

                    if (instance != null)
                    {
                        Console.WriteLine("Parser: " + instance.GetType().ToString());
                        Console.WriteLine(instance.ToString());

                        Console.WriteLine("--- Dane szczegółowe --- ");
                        Console.WriteLine("Mime: " + instance.MimeType);
                        Console.WriteLine("FileFormat: " + instance.FileFormat);
                        Console.WriteLine("Duration: " + instance.Duration.ToString());
                        Console.WriteLine("Time: " + PSNC.Multimedia.Tools.MediaParserTools.GetHumanReadableDuration((long)instance.Duration));
                        Console.WriteLine("Filelength: " + instance.Filelength.ToString());
                        Console.WriteLine("Bitrate: " + instance.Bitrate.ToString());

                        Console.WriteLine("- Audio - ");
                        if (instance.AudioStreams != null)
                        {
                            int a = 0;
                            foreach (AudioStreamProperties audio in instance.AudioStreams)
                            {
                                if (audio != null)
                                {
                                    a++;
                                    Console.WriteLine(a.ToString());
                                    Console.WriteLine("Bitrate: " + audio.Bitrate.ToString());
                                    Console.WriteLine("BitsPerSample: " + audio.BitsPerSample.ToString());
                                    Console.WriteLine("Codec: " + audio.Codec);
                                    Console.WriteLine("IsVbr: " + audio.IsVbr.ToString());
                                    Console.WriteLine("Layer: " + audio.Layer.ToString());
                                    Console.WriteLine("SamplesPerSec: " + audio.SamplesPerSec.ToString());
                                    Console.WriteLine("WaveFormat: " + audio.WaveFormat.ToString());
                                    Console.WriteLine("Coding: " + audio.Coding);
                                }
                            }
                        }

                        Console.WriteLine("- Video - ");
                        if (instance.VideoStreams != null)
                        {
                            int a = 0;
                            foreach (VideoStreamProperties video in instance.VideoStreams)
                            {
                                if (video != null)
                                {
                                    a++;
                                    Console.WriteLine(a.ToString());
                                    Console.WriteLine("Bitrate: " + video.Bitrate.ToString());
                                    Console.WriteLine("Coding: " + video.Coding);
                                }
                            }
                        }

                        Console.WriteLine("- Metadata - ");
                        if (instance.Metadata != null)
                        {
                            foreach (KeyValuePair<string, string> pair in instance.Metadata)
                            {
                                Console.WriteLine(pair.Key + ": " + pair.Value);
                            }
                        }
                        if (instance.Warnings != null && instance.Warnings.Any())
                        {
                            Console.WriteLine("- Warnings - ");
                            foreach (var warning in instance.Warnings)
                            {
                                Console.WriteLine(warning);
                            }
                        }
                        var dic = new PSNC.Multimedia.Tools.DictionaryEx<string, object>();
                        var xml = instance.ToXML(dic);
                        Console.WriteLine(xml);
                    }
                    else Console.WriteLine("Nieznana zawartość pliku " + path);

                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine(ex.Source);
            }
#if DEBUG
            Console.Write("Press any key to exit");
            Console.ReadKey();
#endif
        }
    }
}
