/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: ListenerList.cs,v 1.1 2010/11/19 15:40:39 kzmi Exp $
 */
using System;
using System.Collections;
using System.Collections.Generic;

//namespace Poderosa.Util
namespace Telnet
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <exclude/>
	public class ListenerList<T> : IEnumerable<T> {
        private LinkedList<T> _list;

		public ListenerList() {
		}
		public void Add(T listener) {
            Precheck();
            _list.AddLast(listener);
		}
        public void Remove(T listener) {
            Precheck();
            _list.Remove(listener);
        }

		IEnumerator IEnumerable.GetEnumerator() {
            Precheck();
            return _list.GetEnumerator();
		}
		IEnumerator<T> IEnumerable<T>.GetEnumerator() {
            Precheck();
            return _list.GetEnumerator();
		}

        public bool IsEmpty {
            get {
                return _list!=null && _list.Count==0;
            }
        }

        public void Clear() {
            if(_list!=null) _list.Clear();
        }

        //‘½‚­‚Íˆê‚Â‚àListener‚ª“o˜^‚³‚ê‚È‚¢B’x‰„ì¬‚·‚é
        private void Precheck() {
            if (_list == null) _list = new LinkedList<T>();
        }

	}

    //ƒŠƒXƒi‚Ì“o˜^EíœƒCƒ“ƒ^ƒtƒF[ƒX
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <exclude/>
    public interface IListenerRegistration<T> {
        void AddListener(T listener);
        void RemoveListener(T listener);
    }
}
