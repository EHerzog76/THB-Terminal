using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

using Core;
using ConnectionParam;

namespace TerminalEmulator
{
    public class TerminalDocument : IDisposable
    {
        private int _scrollingTop;
        private int _scrollingBottom;

        // buffer
        private byte[] buffer = null;
        private VirtualScreen virtualScreen = null;
        // width of vs
        private int vsWidth = 0;
        // height of vs
        private int vsHeight = 0;
        private int timeoutReceive = 3; // timeout in seconds
        private int _debug = 0;
        protected bool _bApplicationMode;
        //const string ENDOFLINE = "\r\n"; // CR LF
        const int SCREENXNULLCOORDINATE = 0;
        const int SCREENYNULLCOORDINATE = 0;
        const int TRAILS = 5; // 25 / trails until timeout in "wait"-methods
        const byte CR = 13;
        const byte LF = 10;
        private bool disposed = false;

        public VirtualScreen VirtualScreen
        {
            get
            {
                return this.virtualScreen;
            }
        }

        internal TerminalDocument(int width, int height)
        {
            this.vsWidth = width;
            this.vsHeight = height;
            // virtual screen
            if (this.virtualScreen == null)
                this.virtualScreen = new Core.VirtualScreen(width, height, 1, 1);
            Resize(width, height);
            Clear();
        }

        ~TerminalDocument()
        {
            Dispose(false);
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    this.virtualScreen = null;
                    this.buffer = null;
                }

                //free unmanaged objects
                //AdditionalCleanup();

                this.disposed = true;
            }
        }

        public int TerminalHeight
        {
            get
            {
                return vsHeight;
            }
        }
        public int TerminalWidth
        {
            get
            {
                return vsWidth;
            }
        }
        public bool IsApplicationMode
        {
            get
            {
                return _bApplicationMode;
            }
            set
            {
                _bApplicationMode = value;
            }
        }

        public string ShowScreen()
        {
            return virtualScreen.Hardcopy();
        }

        public void Clear()
        {
            /*
            _caretColumn = 0;
            _firstLine = null;
            _lastLine = null;
            _size = 0;
            AddLine(new GLine(_width));
            */
        }
        public void Resize(int width, int height)
        {
            vsWidth = width;
            vsHeight = height;
        }
        public void RemoveAfterCaret()
        {
            this.CleanAfterCaret();
        }
        public void CleanAfterCaret()
        {
            this.virtualScreen.CleanLine(this.virtualScreen.CursorX, vsWidth);
        }
        public void CleanLineRange(int xStart, int xEnd)
        {
            this.virtualScreen.CleanLine(xStart, xEnd);
        }
        public void ClearAfter(int yStart)
        {
            this.virtualScreen.CleanScreen(0, yStart, vsWidth, this.virtualScreen.TopVisibleLineNumber + vsHeight);
        }
        public void ClearRange(int yStart, int yEnd)
        {
            this.virtualScreen.CleanScreen(0, yStart, vsWidth, yEnd);
        }
        public void MoveCursor(int cols)
        {
            //Scroll-UP/-Down is considered if cols > TerminalWidth
            this.virtualScreen.MoveCursor(cols);
        }
        public void MoveCursorVertical(int rows)
        {
            this.virtualScreen.MoveCursorVertical(rows);
        }
        public void MoveCursorTo(int cols, int rows)
        {
            this.virtualScreen.MoveCursorTo(cols, rows);
        }
        public void SetCursorPos(int xPos, int yPos)
        {
            this.virtualScreen.CursorX = xPos;
            this.virtualScreen.CursorY = yPos;
        }
        public void SetScrollingRegion(int top_offset, int bottom_offset)
        {
            //Console.WriteLine("TerminalDocument: SetScrollingRegion is not implemented!");
            return;

            //_scrollingTop = TopLineNumber + top_offset;
            //_scrollingBottom = TopLineNumber + bottom_offset;
        }
        public void ClearScrollingRegion()
        {
            if(_debug > 0)
                Console.WriteLine("TerminalDocument: ClearScrollingRegion is not implemented!");
            return;

            ////_scrollingTop = -1;
            ////_scrollingBottom = -1;
            //this.virtualScreen.CursorReset();
            ////this.virtualScreen.CleanScreen();
        }
        public int DebugFlag
        {
            get
            {
                return _debug;
            }
            set
            {
                _debug = value;
                this.virtualScreen.DebugFlag = _debug;
            }
        }
        internal void ScrollUp()
        {
            if(_debug > 0)
                Console.WriteLine("TerminalDocument: ScrollUp is not implemented!");
            return;
            /*
            if (_scrollingTop != -1 && _scrollingBottom != -1)
                ScrollUp(_scrollingTop, _scrollingBottom);
            else
                ScrollUp(TopLineNumber, TopLineNumber + vsHeight - 1);
            */
        }

        internal void ScrollUp(int from, int to)
        {
            if (_debug > 0)
                Console.WriteLine("TerminalDocument: ScrollUp is not implemented!");
            return;
        }

        //ƒXƒNƒ[ƒ‹”ÍˆÍ‚ÌÅ‚àã‚ð‚PsÁ‚µAÅ‚à‰º‚É‚Ps’Ç‰ÁBŒ»Ýs‚Í‚»‚ÌV‹Ks‚É‚È‚éB
        internal void ScrollDown()
        {
            if (_debug > 0)
                Console.WriteLine("TerminalDocument: ScrollDown is not implemented!");
            return;
            /*
            if (_scrollingTop != -1 && _scrollingBottom != -1)
                ScrollDown(_scrollingTop, _scrollingBottom);
            else
                ScrollDown(TopLineNumber, TopLineNumber + vsHeight - 1);
            */
        }

        internal void ScrollDown(int from, int to)
        {
            if (_debug > 0)
                Console.WriteLine("TerminalDocument: ScrollDown is not implemented!");
            return;
        }
        public int ScrollingTop
        {
            get
            {
                return this.virtualScreen.TopVisibleLineNumber;
                //return _scrollingTop;
            }
        }
        public int ScrollingBottom
        {
            get
            {
                return _scrollingBottom;
            }
        }

        public int TopLineNumber
        {
            get
            {
                return this.virtualScreen.TopVisibleLineNumber;
            }
        }
        public int CurrentLineNumber
        {
            get
            {
                return this.virtualScreen.CursorY;
            }
            set
            {
                this.virtualScreen.CursorY = value;
            }
        }
        public string CurrentLine
        {
            get
            {
                if (this.virtualScreen != null)
                    return this.virtualScreen.GetLine(this.virtualScreen.CursorY);
                else
                    return (null);
            }
        }
        public int CaretColumn
        {
            get
            {
                return this.virtualScreen.CursorX;
            }
            set
            {
                this.virtualScreen.CursorX = value;
            }
        }

        internal void PutChar(char ch) {
            this.virtualScreen.Write(ch);
        }
        public LineFeedRule LineFeedRule
        {
            get
            {
                return this.virtualScreen.LineFeedRule;
            }
            set
            {
                this.virtualScreen.LineFeedRule = value;
            }
        }
        internal void LineFeed()
        {
            //nl.EOLType = (nl.EOLType == EOLType.CR || nl.EOLType == EOLType.CRLF) ? EOLType.CRLF : EOLType.LF;
            //this.virtualScreen.WriteByte(CR);
            this.virtualScreen.WriteByte(LF);

            //Debug.WriteLine(String.Format("c={0} t={1} f={2} l={3}", _currentLine.ID, _topLine.ID, _firstLine.ID, _lastLine.ID));
        }
        internal void CarriageReturn()
        {
            this.virtualScreen.WriteByte(CR);
        }
        internal void TabStop()
        {
            this.virtualScreen.WriteByte(0x09);
        }

        #region WaitFor-methods
        /// <summary>
        /// Wait for a particular strings
        /// </summary>
        /// <param name="searchFor">string-array to be found</param>
        /// <returns>string found or null if not found</returns>
        public string WaitFor(string[] searchFor, bool caseSensitive, int timeoutSeconds)
        {
            if (this.virtualScreen == null || searchFor == null || searchFor.Length < 1)
                return null;
            // use the appropriate timeout setting, which is the smaller number
            int sleepTimeMs = this.GetWaitSleepTimeMs(timeoutSeconds);
            DateTime endTime = this.TimeoutAbsoluteTime(timeoutSeconds);
            string found = null;
            do
            {
                foreach(string strSearch in searchFor){
                    lock (this.virtualScreen)
                    {
                        found = this.virtualScreen.FindOnScreen(strSearch, caseSensitive);
                    }
                    if (found != null)
                        return found;
                }
                Thread.Sleep(sleepTimeMs);
            } while (DateTime.Now <= endTime);
            return found;
        }
        /// <summary>
        /// Wait for a particular string
        /// </summary>
        /// <param name="searchFor">string to be found</param>
        /// <returns>string found or null if not found</returns>
        public string WaitForString(string searchFor)
        {
            return this.WaitForString(searchFor, false, this.timeoutReceive);
        }

        /// <summary>
        /// Wait for a particular string
        /// </summary>
        /// <param name="searchFor">string to be found</param>
        /// <param name="caseSensitive">case sensitive search</param>
        /// <param name="timeoutSeconds">timeout [s]</param>
        /// <returns>string found or null if not found</returns>
        public string WaitForString(string searchFor, bool caseSensitive, int timeoutSeconds)
        {
            if (this.virtualScreen == null || searchFor == null || searchFor.Length < 1)
                return null;
            // use the appropriate timeout setting, which is the smaller number
            int sleepTimeMs = this.GetWaitSleepTimeMs(timeoutSeconds);
            DateTime endTime = this.TimeoutAbsoluteTime(timeoutSeconds);
            string found = null;
            do
            {
                lock (this.virtualScreen)
                {
                    found = this.virtualScreen.FindOnScreen(searchFor, caseSensitive);
                }
                if (found != null)
                    return found;
                Thread.Sleep(sleepTimeMs);
            } while (DateTime.Now <= endTime);
            return found;
        }

        /// <summary>
        /// Wait for a particular regular expression
        /// </summary>
        /// <param name="regEx">string to be found</param>
        /// <returns>string found or null if not found</returns>
        public string WaitForRegEx(string regEx)
        {
            return this.WaitForRegEx(regEx, this.timeoutReceive);
        }

        /// <summary>
        /// Wait for a particular regular expression
        /// </summary>
        /// <param name="regEx">string to be found</param>
        /// <param name="timeoutSeconds">timeout [s]</param>
        /// <returns>string found or null if not found</returns>
        public string WaitForRegEx(string regEx, int timeoutSeconds)
        {
            if (this.virtualScreen == null || regEx == null || regEx.Length < 1)
                return null;
            int sleepTimeMs = this.GetWaitSleepTimeMs(timeoutSeconds);
            DateTime endTime = this.TimeoutAbsoluteTime(timeoutSeconds);
            string found = null;
            do // at least once
            {
                lock (this.virtualScreen)
                {
                    found = this.virtualScreen.FindRegExOnScreen(regEx);
                }
                if (found != null)
                    return found;
                Thread.Sleep(sleepTimeMs);
            } while (DateTime.Now <= endTime);
            return found;
        }

        /// <summary>
        /// Wait for changed screen. Read further documentation 
        /// on <code>WaitForChangedScreen(int)</code>.
        /// </summary>
        /// <returns>changed screen</returns>
        public bool WaitForChangedScreen()
        {
            return this.WaitForChangedScreen(this.timeoutReceive);
        }

        /// <summary>
        /// Waits for changed screen: This method here resets
        /// the flag of the virtual screen and afterwards waits for
        /// changes.
        /// <p>
        /// This means the method detects changes after the call
        /// of the method, NOT prior.
        /// </p>
        /// <p>
        /// To reset the flag only use <code>WaitForChangedScreen(0)</code>.
        /// </p>
        /// </summary>
        /// <param name="timeoutSeconds">timeout [s]</param>
        /// <remarks>
        /// The property ChangedScreen of the virtual screen is
        /// reset after each call of Hardcopy(). It is also false directly
        /// after the initialization.
        /// </remarks>
        /// <returns>changed screen</returns>
        public bool WaitForChangedScreen(int timeoutSeconds)
        {
            // 1st check
            if (this.virtualScreen == null || timeoutSeconds < 0)
                return false;

            // reset flag: This has been added after the feedback of Mark
            if (this.virtualScreen.ChangedScreen)
                this.virtualScreen.Hardcopy(false);

            // Only reset
            if (timeoutSeconds <= 0)
                return false;

            // wait for changes, the goal is to test at TRAILS times, if not timing out before
            int sleepTimeMs = this.GetWaitSleepTimeMs(timeoutSeconds);
            DateTime endTime = this.TimeoutAbsoluteTime(timeoutSeconds);
            do // run at least once
            {
                lock (this.virtualScreen)
                {
                    if (this.virtualScreen.ChangedScreen)
                        return true;
                }
                Thread.Sleep(sleepTimeMs);
            } while (DateTime.Now <= endTime);
            return false;
        } // WaitForChangedScreen

        /// <summary>
        /// Wait (=Sleep) for n seconds
        /// </summary>
        /// <param name="seconds">seconds to sleep</param>
        public void Wait(int seconds)
        {
            if (seconds > 0)
                Thread.Sleep(seconds * 1000);
        } // Wait

        /// <summary>
        /// Helper method: 
        /// Get the appropriate timeout, which is the bigger number of
        /// timeoutSeconds and this.timeoutReceive (TCP client timeout)
        /// </summary>
        /// <param name="timeoutSeconds">timeout in seconds</param>
        private int GetWaitTimeout(int timeoutSeconds)
        {
            if (timeoutSeconds < 0 && this.timeoutReceive < 0)
                return 0;
            else if (timeoutSeconds < 0)
                return this.timeoutReceive; // no valid timeout, return other one
            else
                return (timeoutSeconds >= this.timeoutReceive) ? timeoutSeconds : this.timeoutReceive;
        }

        /// <summary>
        /// Helper method: 
        /// Get the appropriate sleep time based on timeout and TRIAL
        /// </summary>
        /// <param name="timeoutSeconds">timeout ins seconds</param>
        private int GetWaitSleepTimeMs(int timeoutSeconds)
        {
            int tm = (this.GetWaitTimeout(timeoutSeconds) * 1000) / TRAILS;

            if(tm > 600)
                tm = 600;
            return tm;
        }

        /// <summary>
        /// Helper method: 
        /// Get the end time, which is "NOW" + timeout
        /// </summary>
        /// <param name="timeoutSeconds">timeout int seconds</param>
        private DateTime TimeoutAbsoluteTime(int timeoutSeconds)
        {
            return DateTime.Now.AddSeconds(this.GetWaitTimeout(timeoutSeconds));
        }
        #endregion
    }
}
