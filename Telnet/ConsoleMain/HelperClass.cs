using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace ConsoleMain
{
    static class HelperClass
    {
        public static string quotemeta(string strSource) {
            Regex re = new Regex("([^A-Za-z0-9 ~@])");  // @"([\.\$\^\{\[\(\|\)\*\+\?\\])"
            
            return(re.Replace(strSource, @"\$1"));
        }

        public static bool RegExCompare(string Source, string Pattern, RegexOptions options)
        {
            Regex re = new Regex(Pattern, options);
            
            return(re.IsMatch(Source));
        }

        public static Hashtable ReadConf(string l_ConfigFile)
        {
            Hashtable l_Config = new Hashtable();
            string[] configs = null;
            string config = "", cur_line = "", next_line = "";
            Regex re = null;
  
	        if (!File.Exists(l_ConfigFile) )
                return(null);

            using (TextReader reader = File.OpenText(l_ConfigFile))
	        {

                configs = reader.ReadToEnd().Split(new char[] { '\n' });
	        }

	        for(int LineNr = 0; LineNr < configs.Length; LineNr++) {
	            config = configs[LineNr].Replace("\n", "");
                config = config.Replace("\r", "");

                // Check for \# =>  \Route~
                config = config.Replace("\\#", "\\Route~");
                // Take out Comments
                re = new Regex("^([^#]*)#.*$");
                config = re.Replace(config, "$1");  //config =~ s/^([^#]*)#.*$/$1/;
    	  
                // Check for wrap (\ at end of line)
                cur_line = config;
                while (RegExCompare(cur_line, @"\\[\s\t]*$", RegexOptions.None)) {
                    // Remove tailing \
                    re = new Regex(@"\\[\s\t]*$");
                    config = re.Replace(config, "");    //config =~ s/\\[\s\t]*$//;

                    // Trim trailing white space
                    re = new Regex(@"[\s\t]*$");
                    config = re.Replace(config, "");    //config =~ s/[\s\t]*$//g;

                    //Grab next line
                    LineNr ++;
                    next_line = configs[LineNr].Replace("\n", "");
                    next_line = next_line.Replace("\r", "");        //chomp($next_line);
    
                    //Check for \# =>  \Route~
                    next_line = next_line.Replace("\\#", "\\Route~");
                    // Take out Comments
                    re = new Regex("^([^#]*)#.*$");
                    next_line = re.Replace(next_line, "$1");    //next_line =~ s/ ^([^#]*)#.*$/$1/;
        
                    //could have lots of \'d lines in a row
                    cur_line = next_line;
        
                    // Check for trailing \ on newline
                    if ( RegExCompare(next_line, @"\\[\s\t]*$", RegexOptions.None) ) {
                        re = new Regex(@"\\[\s\t]*$");
                        next_line = re.Replace(next_line, "");
                    }
                    //Trim leading and trailing white space
                    next_line = next_line.Trim(); // s/^[\s\t]*//g; next_line =~ s/[\s\t]*$//g;

                    // concat next line to current.
                    config += next_line;
                }

                //Take out Comments
                re = new Regex("^([^#]*)#.*$");
                config = re.Replace(config, "$1");
    
                //Check for \Route~ =>  Change it back to: #
                config = config.Replace("\\Route~", "#");
    
                //Trim leading and trailing white space
                config = config.Trim();
                // Ignore Blank Lines
                if(config.Length == 0)
                    continue;
    
                //Fill the CONFIG hash
                if (RegExCompare(config, @"^([a-zA-Z_\-0-9]+)\s*=\s*(.*)$", RegexOptions.None) ) {
                    string[] ValuePair = config.Split(new char[] { '=' });
                    if(ValuePair.Length != 2) {
                        Console.WriteLine("Bad Config Line : " + config);
                    } else {
                        ValuePair[0] = ValuePair[0].Trim();
                        ValuePair[1] = ValuePair[1].Trim();

                        l_Config.Add(ValuePair[0], ValuePair[1]);
                    }
                }
            }

            // Set Default Values if no Values defined:
            if( !l_Config.ContainsKey("Timeout") )
                l_Config.Add("Timeout", 90);
            else if(l_Config["Timeout"].ToString().Length == 0)
                l_Config["Timeout"] = 90;

            return (l_Config);
        }
    }
}
