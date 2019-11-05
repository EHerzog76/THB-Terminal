/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: KeepAlive.cs,v 1.2 2010/12/02 16:20:21 kzmi Exp $
 */
using System;
using System.Diagnostics;
//using System.Windows.Forms;

using Core;
using TerminalEmulator;

namespace TerminalSessions
{
	public class KeepAlive {
        private Object _parent;
        private Timer _timer;
        private int _prevInterval;

		public KeepAlive(Object parentObj) {
            _parent = parentObj;
            _prevInterval = 0;
		}

        public void Refresh(int interval) {
            bool first = false;
            if(_timer==null) {
                first = true;
                _timer = new Timer();
                //_timer.Tick += new EventHandler(OnTimer);
                _timer.OnTimerTick += new Timer.Tick(OnTimer);
            }

            if(!first && _prevInterval==interval) return; //Šù‘¶Ý’è‚Æ•ÏX‚Ì‚È‚¢ê‡‚Í‰½‚à‚µ‚È‚¢

            if(interval>0) {
                _timer.Interval = interval;
                _timer.Start();
            }
            else
                _timer.Stop();

            _prevInterval = interval;
        }


		void OnTimer(object sender, EventArgs args) {
            //TODO ƒAƒvƒŠƒP[ƒVƒ‡ƒ““à•”ƒCƒxƒ“ƒgƒƒO‚Å‚àì‚Á‚Ä‚±‚¤‚¢‚¤‚Ì‚Í‹L˜^‚µ‚Ä‚¢‚­‚Ì‚ª‚¢‚¢‚Ì‚©H
            //foreach(ISession s in TerminalEmulatorPlugin.Instance.GetSessionManager().AllSessions) {
            //    IAbstractTerminalHost ts = (IAbstractTerminalHost)s.GetAdapter(typeof(IAbstractTerminalHost));
            //    if (ts != null && ts.TerminalConnection != null && ts.TerminalConnection.TerminalOutput != null) {
            //        ts.TerminalConnection.TerminalOutput.SendKeepAliveData();
            //    }
            //}
            /* //removed 25.06.2015
            if(_parent.GetType() == typeof(TerminalEmulatorPlugin)) {
                //((TerminalEmulatorPlugin)_parent).KeepAlive;
            }
            else */
            if (_parent.GetType() == typeof(TerminalSession))  //ITerminalSession
            {
                TerminalSession ts = (TerminalSession)_parent;
                if (ts != null && ts.TerminalConnection != null && ts.TerminalConnection.TerminalOutput != null) {
                    ts.TerminalConnection.TerminalOutput.SendKeepAliveData();
                }
            }
        }


    }
}
