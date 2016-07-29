using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PSNC.Multimedia
{
    public static class VttParser
    {

        public enum VttParts { Unknown, Header, Empty, Identifier, Time, Text }

        static readonly Regex RegexTimeCodes = new Regex(@"^(^([0-9][0-9]):([0-5][0-9]):([0-5][0-9])\.-?\d+\s*-->\s*-?([0-9][0-9]):([0-5][0-9]):([0-5][0-9])\.-?\d+)", RegexOptions.Compiled);
        static readonly Regex RegexTimeCodesShort = new Regex(@"^(^([0-5][0-9]):([0-5][0-9])\.-?\d+\s*-->\s*-?([0-5][0-9]):([0-5][0-9])\.-?\d+)", RegexOptions.Compiled);
        static readonly Regex RegexCounter = new Regex(@"^\d+$", RegexOptions.Compiled);

        public static bool LoadSubtitle(List<string> lines, List<string> warnings, ref TimeSpan highestTime)
        {
            if (warnings == null) warnings = new List<string>();
            bool result = true;
            int linesCount = 0;
            string currentIdentifier = String.Empty;
            string previousIdentifier = String.Empty;
            var errorCount = 0;
            Cue cue = null;
            Cue previousCue = null;
            bool textDone = true;
            VttParts currentPart = VttParts.Unknown;
            VttParts previousPart = VttParts.Unknown;
            foreach (string line in lines)
            {
                currentPart = VttParts.Unknown;
                string s = line;
                linesCount++;

                if (RegexTimeCodesShort.IsMatch(s))
                {
                    currentPart = VttParts.Time;
                    s = "00:" + s.Replace("--> ", "--> 00:");
                }

                if (RegexTimeCodes.IsMatch(s))
                {
                    currentPart = VttParts.Time;
                    textDone = false;
                    if (cue != null)
                    {
                        cue = null;
                    }
                    try
                    {
                        string[] parts = s.Replace("-->", "@").Split("@".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        cue = new Cue();
                        cue.Identifier = previousIdentifier;
                        cue.StartTime = TimeCode.FromString(parts[0]);
                        cue.EndTime = TimeCode.FromString(parts[1]);
                        if (cue.EndTime.TimeSpan < cue.StartTime.TimeSpan)
                        {
                            HandleError(String.Format("Koniec napisów przed początkiem - linia nr {0} : {1}", linesCount, s));
                            errorCount++;
                        }
                        if (previousCue != null)
                        {
                            if (previousCue.StartTime.TimeSpan > cue.StartTime.TimeSpan)
                            {
                                HandleError(String.Format("Zły czas napisów w porównaniu z poprzednim wpisem -  linia nr {0} : {1}", linesCount, s));
                                errorCount++;
                            }
                        }
                        if (cue.EndTime.TimeSpan > highestTime)
                            highestTime = cue.EndTime.TimeSpan;
                    }
                    catch (Exception exception)
                    {
                        HandleError(exception.Message);
                        errorCount++;
                        cue = null;
                    }
                }
                else if (linesCount == 1 && line.Trim() == "WEBVTT")
                {
                    currentPart = VttParts.Header;
                }
                else if (cue != null && line.Trim().Length > 0)
                {
                    currentPart = VttParts.Text;
                    string text = line.Trim();
                    if (!textDone)
                        cue.Text = (cue.Text + Environment.NewLine + text).Trim();
                }
                else if (line.Trim().Length > 0 && previousPart == VttParts.Empty)
                {
                    currentPart = VttParts.Identifier;
                    currentIdentifier = line;
                    if (RegexCounter.IsMatch(currentIdentifier) && RegexCounter.IsMatch(previousIdentifier))
                    {
                        var currentIdentifierAsInt = 0;
                        var previousIdntifierAsInt = 0;
                        if (int.TryParse(currentIdentifier, out currentIdentifierAsInt) && int.TryParse(previousIdentifier, out previousIdntifierAsInt))
                        {
                            if (currentIdentifierAsInt <= previousIdntifierAsInt)
                            {
                                HandleError(String.Format("Błędny licznik wpisu linia nr {0} : {1} ( poprzedni: {2})", linesCount, currentIdentifier, previousIdentifier));
                                errorCount++;
                            }
                        }
                    }
                    previousIdentifier = currentIdentifier;
                }
                else if (line.Length == 0)
                {
                    currentPart = VttParts.Empty;
                    textDone = true;
                    if (cue != null)
                    {
                        previousCue = cue;
                        cue = null;
                    }
                }
                else
                {
                    if (line.Contains("-->"))
                        currentPart = VttParts.Time;
                    if (linesCount == 1 && line.StartsWith("WEBVTT "))
                        currentPart = VttParts.Header;
                    if (currentPart == VttParts.Time)
                    {
                        var error = String.Format("Błędny format linii nr {0} : {1}", linesCount, s);
                        HandleError(error);
                        errorCount++;
                        warnings.Add(error);
                        result = false;
                    }
                }
                previousPart = currentPart;
                if (cue != null)
                {
                    previousCue = cue;
                }
            }
            return result;
        }

        private static void HandleError(string error)
        {
            Console.WriteLine(error);
            Debug.WriteLine(error);
        }

    }

    public static class SrtParser
    {

        public enum SrtParts { Unknown, Empty, Identifier, Time, Text }

        static readonly Regex RegexTimeCodes = new Regex(@"^([0-9][0-9]):([0-5][0-9]):([0-5][0-9])\,-?\d+\s*-->\s*-?([0-9][0-9]):([0-5][0-9]):([0-5][0-9])\,-?\d+$", RegexOptions.Compiled);
        static readonly Regex RegexTimeCodesShort = new Regex(@"^([0-5][0-9]):([0-5][0-9])\,-?\d+\s*-->\s*-?([0-5][0-9]):([0-5][0-9])\,-?\d+$", RegexOptions.Compiled);
        static readonly Regex RegexCounter = new Regex(@"^\d+$", RegexOptions.Compiled);

        public static bool LoadSubtitle(List<string> lines, List<string> warnings, ref TimeSpan highestTime)
        {
            if (warnings == null) warnings = new List<string>();
            bool result = true;
            int linesCount = 0;
            string currentIdentifier = String.Empty;
            string previousIdentifier = String.Empty;
            var errorCount = 0;
            Cue cue = null;
            Cue previousCue = null;
            bool textDone = true;
            SrtParts currentPart = SrtParts.Unknown;
            SrtParts previousPart = SrtParts.Unknown;
            foreach (string line in lines)
            {
                currentPart = SrtParts.Unknown;
                string s = line;
                linesCount++;

                if (RegexTimeCodesShort.IsMatch(s))
                {
                    currentPart = SrtParts.Time;
                    s = "00:" + s.Replace("--> ", "--> 00:");
                }

                if (RegexTimeCodes.IsMatch(s))
                {
                    currentPart = SrtParts.Time;
                    textDone = false;
                    if (cue != null)
                    {
                        cue = null;
                    }
                    try
                    {
                        string[] parts = s.Replace("-->", "@").Split("@".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        cue = new Cue();
                        cue.Identifier = previousIdentifier;
                        cue.StartTime = TimeCode.FromString(parts[0]);
                        cue.EndTime = TimeCode.FromString(parts[1]);
                        if (cue.EndTime.TimeSpan < cue.StartTime.TimeSpan)
                        {
                            HandleError(String.Format("Koniec napisów przed początkiem - linia nr {0} : {1}", linesCount, s));
                            errorCount++;
                        }
                        if (previousCue != null)
                        {
                            if (previousCue.StartTime.TimeSpan > cue.StartTime.TimeSpan)
                            {
                                HandleError(String.Format("Zły czas napisów w porównaniu z poprzednim wpisem -  linia nr {0} : {1}", linesCount, s));
                                errorCount++;
                            }
                        }
                        if (cue.EndTime.TimeSpan > highestTime)
                            highestTime = cue.EndTime.TimeSpan;
                    }
                    catch (Exception exception)
                    {
                        HandleError(exception.Message);
                        errorCount++;
                        cue = null;
                    }
                }
                else if (cue != null && line.Trim().Length > 0)
                {
                    currentPart = SrtParts.Text;
                    string text = line.Trim();
                    if (!textDone)
                        cue.Text = (cue.Text + Environment.NewLine + text).Trim();
                }
                else if (line.Trim().Length > 0 && previousPart == SrtParts.Empty)
                {
                    currentPart = SrtParts.Identifier;
                    currentIdentifier = line;
                    if (RegexCounter.IsMatch(currentIdentifier) && RegexCounter.IsMatch(previousIdentifier))
                    {
                        var currentIdentifierAsInt = 0;
                        var previousIdntifierAsInt = 0;
                        if (int.TryParse(currentIdentifier, out currentIdentifierAsInt) && int.TryParse(previousIdentifier, out previousIdntifierAsInt))
                        {
                            if (currentIdentifierAsInt <= previousIdntifierAsInt)
                            {
                                HandleError(String.Format("Błędny licznik wpisu linia nr {0} : {1} ( poprzedni: {2})", linesCount, currentIdentifier, previousIdentifier));
                                errorCount++;
                            }
                        }
                    }
                    previousIdentifier = currentIdentifier;
                }
                else if (line.Length == 0)
                {
                    currentPart = SrtParts.Empty;
                    textDone = true;
                    if (cue != null)
                    {
                        previousCue = cue;
                        cue = null;
                    }
                }
                else
                {
                    if (line.Contains("-->"))
                        currentPart = SrtParts.Time;
                    if (currentPart == SrtParts.Time)
                    {
                        var error = String.Format("Błędny format linii nr {0} : {1}", linesCount, s);
                        HandleError(error);
                        errorCount++;
                        warnings.Add(error);
                    }
                }
                previousPart = currentPart;
                if (cue != null)
                {
                    previousCue = cue;
                }
            }
            return result;
        }

        private static void HandleError(string error)
        {
            Console.WriteLine(error);
            Debug.WriteLine(error);
        }

    }

    public class AssParser
    {
        public static string Errors { get; private set; }

        public static bool LoadSubtitle(List<string> lines, List<string> warnings, ref TimeSpan highestTime)
        {
            var result = true;
            var _errorCount = 0;
            Errors = null;
            bool eventsStarted = false;
            bool fontsStarted = false;
            bool graphicsStarted = false;
            string[] format = "Layer, Start, End, Style, Actor, MarginL, MarginR, MarginV, Effect, Text".Split(',');
            int indexLayer = 0;
            int indexStart = 1;
            int indexEnd = 2;
            int indexStyle = 3;
            int indexActor = 4;
            int indexEffect = 8;
            int indexText = 9;
            var errors = new StringBuilder();
            int lineNumber = 0;

            var header = new StringBuilder();
            var footer = new StringBuilder();
            foreach (string line in lines)
            {
                lineNumber++;
                if (!eventsStarted && !fontsStarted && !graphicsStarted)
                    header.AppendLine(line);

                if (line.Trim().Length == 0)
                {

                }
                else if (!string.IsNullOrEmpty(line) && line.Trim().StartsWith(";"))
                {

                }
                else if (line.Trim().ToLower().StartsWith("dialogue:"))
                {
                    eventsStarted = true;
                    fontsStarted = false;
                    graphicsStarted = false;
                }

                if (line.Trim().ToLower() == "[events]")
                {
                    eventsStarted = true;
                    fontsStarted = false;
                    graphicsStarted = false;
                }
                else if (line.Trim().ToLower() == "[fonts]")
                {
                    eventsStarted = false;
                    fontsStarted = true;
                    graphicsStarted = false;
                    footer.AppendLine();
                    footer.AppendLine("[Fonts]");
                }
                else if (line.Trim().ToLower() == "[graphics]")
                {
                    eventsStarted = false;
                    fontsStarted = false;
                    graphicsStarted = true;
                    footer.AppendLine();
                    footer.AppendLine("[Graphics]");
                }
                else if (fontsStarted)
                {
                    footer.AppendLine(line);
                }
                else if (graphicsStarted)
                {
                    footer.AppendLine(line);
                }
                else if (eventsStarted)
                {
                    string s = line.Trim().ToLower();
                    if (s.StartsWith("format:"))
                    {
                        if (line.Length > 10)
                        {
                            format = line.ToLower().Substring(8).Split(',');
                            for (int i = 0; i < format.Length; i++)
                            {
                                if (format[i].Trim().ToLower() == "start")
                                    indexStart = i;
                                else if (format[i].Trim().ToLower() == "end")
                                    indexEnd = i;
                                else if (format[i].Trim().ToLower() == "text")
                                    indexText = i;
                                else if (format[i].Trim().ToLower() == "style")
                                    indexStyle = i;
                                else if (format[i].Trim().ToLower() == "actor")
                                    indexActor = i;
                                else if (format[i].Trim().ToLower() == "effect")
                                    indexEffect = i;
                                else if (format[i].Trim().ToLower() == "layer")
                                    indexLayer = i;
                            }
                        }
                    }
                    else if (!string.IsNullOrEmpty(s))
                    {
                        string text = string.Empty;
                        string start = string.Empty;
                        string end = string.Empty;
                        string style = string.Empty;
                        string actor = string.Empty;
                        string effect = string.Empty;
                        string layer = string.Empty;

                        string[] splittedLine;

                        if (s.StartsWith("dialogue:"))
                            splittedLine = line.Substring(10).Split(',');
                        else
                            splittedLine = line.Split(',');

                        for (int i = 0; i < splittedLine.Length; i++)
                        {
                            if (i == indexStart)
                                start = splittedLine[i].Trim();
                            else if (i == indexEnd)
                                end = splittedLine[i].Trim();
                            else if (i == indexStyle)
                                style = splittedLine[i].Trim();
                            else if (i == indexActor)
                                actor = splittedLine[i].Trim();
                            else if (i == indexEffect)
                                effect = splittedLine[i].Trim();
                            else if (i == indexLayer)
                                layer = splittedLine[i].Trim();
                            else if (i == indexText)
                                text = splittedLine[i];
                            else if (i > indexText)
                                text += "," + splittedLine[i];
                        }

                        try
                        {
                            var p = new Cue();

                            p.StartTime = GetTimeCodeFromString(start);
                            p.EndTime = GetTimeCodeFromString(end);
                            if (p.EndTime.TimeSpan > highestTime)
                                highestTime = p.EndTime.TimeSpan;
                        }
                        catch
                        {
                            _errorCount++;
                        }
                    }
                }
            }
            Errors = errors.ToString();
            return result;
        }

        private static TimeCode GetTimeCodeFromString(string time)
        {
            string[] timeCode = time.Split(':', '.');
            return new TimeCode(int.Parse(timeCode[0]),
            int.Parse(timeCode[1]),
            int.Parse(timeCode[2]),
            int.Parse(timeCode[3]) * 10);
        }

    }

    public class TimeCode
    {
        public static TimeCode MaxTime = new TimeCode(99, 59, 59, 999);

        public static TimeCode MinTime = new TimeCode(0, 0, 0, 0);

        TimeSpan _time;

        public TimeCode(TimeSpan timeSpan)
        {
            TimeSpan = timeSpan;
        }

        public TimeCode(int hour, int minute, int seconds, int milliseconds)
        {
            _time = new TimeSpan(0, hour, minute, seconds, milliseconds);
        }

        public double TotalMilliseconds
        {
            get { return _time.TotalMilliseconds; }
            set { _time = TimeSpan.FromMilliseconds(value); }
        }

        public TimeSpan TimeSpan
        {
            get
            {
                return _time;
            }
            set
            {
                _time = value;
            }
        }

        public override string ToString()
        {
            string s = string.Format("{0:00}:{1:00}:{2:00},{3:000}", _time.Hours, _time.Minutes, _time.Seconds, _time.Milliseconds);
            return s;
        }

        public static TimeCode FromString(string time)
        {
            string[] timeCode = time.Trim().Split(':', '.', ',', ' ');
            return new TimeCode(int.Parse(timeCode[0]),
            int.Parse(timeCode[1]),
            int.Parse(timeCode[2]),
            int.Parse(timeCode[3]));
        }

    }

    public class Cue
    {
        public string Identifier { get; set; }

        public string Text { get; set; }

        public TimeCode StartTime { get; set; }

        public TimeCode EndTime { get; set; }

        public TimeCode Duration
        {
            get
            {
                return new TimeCode(EndTime.TimeSpan - StartTime.TimeSpan);
            }
        }

        public Cue()
        {
            Identifier = String.Empty;
            StartTime = new TimeCode(TimeSpan.FromSeconds(0));
            EndTime = new TimeCode(TimeSpan.FromSeconds(0));
            Text = String.Empty;
        }

        public override string ToString()
        {
            return Identifier + ": " + StartTime + " --> " + EndTime + " " + Text;
        }

    }

}
