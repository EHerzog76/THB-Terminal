/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: WinFormsUtil.cs,v 1.1 2010/11/19 15:41:20 kzmi Exp $
 */
using System;
//using System.Windows.Forms;
using System.Text;
//using System.Drawing;

namespace Core
{
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    //removed 25.06.2015   => Buildaction = NONE
	public class WinFormsUtil
	{
		public static string KeyString(Keys key) {
			int ik = (int)key;
			if((int)Keys.D0<=ik && ik<=(int)Keys.D9)
				return new string((char)('0' + (ik-(int)Keys.D0)), 1);
			else if(key==Keys.None)
                return "";
            else {
                return key.ToString(); 
                /*
				switch(key) {
					case Keys.None:
						return "";
                    //“ÁŽêˆµ‚¢ƒOƒ‹[ƒv
					case Keys.Prior:
						return "PageUp";
					case Keys.Next:
						return "PageDown";
						//Oem‚Ù‚É‚á‚ç‚ç‚ª‚¤‚´‚Á‚½‚¢
					case Keys.OemBackslash:
						return "Backslash";
					case Keys.OemCloseBrackets:
						return "CloseBrackets";
					case Keys.Oemcomma:
						return "Comma";
					case Keys.OemMinus:
						return "Minus";
					case Keys.OemOpenBrackets:
						return "OpenBrackets";
					case Keys.OemPeriod:
						return "Period";
					case Keys.OemPipe:
						return "Pipe";
					case Keys.Oemplus:
						return "Plus";
					case Keys.OemQuestion:
						return "Question";
					case Keys.OemQuotes:
						return "Quotes";
					case Keys.OemSemicolon:
						return "Semicolon";
					case Keys.Oemtilde:
						return "Tilde";
					default:
						return key.ToString();
				}
                 */
			}
		}

        //KeyString‚Ì‹t•ÏŠ·@KeyConverter‚ÌŽÀ‘•‚ÍŽ€‚Ê‚Ù‚Ç’x‚¢
        public static Keys ParseSingleKey(string s) {
            if(s.Length==0)
                return Keys.None;
            else if(s.Length==1) {
                char ch = s[0];
                if('0'<=ch && ch<='9')
                    return Keys.D0 + (ch - '0');
                else
                    return (Keys)Enum.Parse(typeof(Keys), s);
            }
            else {
                //‚Q•¶ŽšˆÈã‚¾‚Á‚½‚ç‘½­try...catch‚ ‚Á‚Ä‚à‚¢‚¢‚¾‚ë‚¤
                try {
                    return (Keys)Enum.Parse(typeof(Keys), s);
                }
                catch(Exception) {
                    if(s=="PageUp")
                        return Keys.Prior;
                    else if(s=="PageDown")
                        return Keys.Next;
                    else
                        return (Keys)Enum.Parse(typeof(Keys), "Oem"+s, true);
                }
            }
        }
        public static Keys ParseKey(string value) {
            return ParseKey(value.Split('+'));
        }
        public static Keys ParseKey(string[] value) { //modifierž‚Ý‚Åƒp[ƒX
            Keys modifier = Keys.None;
            for(int i=0; i<value.Length-1; i++) { //ÅŒãˆÈŠO
                string m = value[i];
                modifier |= ParseModifier(m);
            }
            return modifier | ParseSingleKey(value[value.Length-1]);
        }
        public static Keys ParseModifier(string value) {
            if(value=="Alt" || value=="Menu") return Keys.Alt;
            else if(value=="Shift") return Keys.Shift;
            else if(value=="Ctrl" || value=="Control") return Keys.Control;
            else if(value=="None") return Keys.None;
            else throw new Exception(value + " is unknown modifier");
        }

        public static string FormatShortcut(Keys key) {
            return FormatShortcut(key, '+');
        }
        public static string FormatShortcut(Keys key, char delim) {
            Keys modifiers = key & Keys.Modifiers;
            StringBuilder b = new StringBuilder();
            if((modifiers & Keys.Control)!=Keys.None) {
                b.Append("Ctrl");
            }
            if((modifiers & Keys.Shift)!=Keys.None) {
                if(b.Length>0) b.Append(delim);
                b.Append("Shift");
            }
            if((modifiers & Keys.Alt)!=Keys.None) {
                if(b.Length>0) b.Append(delim);
                b.Append("Alt");
            }
            if(b.Length>0)
                b.Append(delim);

            b.Append(WinFormsUtil.KeyString(key & Keys.KeyCode));
            return b.ToString();
        }

        /*
        public static Control FindTopControl(Control parent, Point screen_pt) {
            Control c = parent.GetChildAtPoint(parent.PointToClient(screen_pt));
            if(c==null) {
                if(parent.RectangleToScreen(new Rectangle(0,0,parent.Width,parent.Height)).Contains(screen_pt))
                    return parent;
                else
                    return null;
            }
            else
                return FindTopControl(c, screen_pt);
        }
        */
    }
}
