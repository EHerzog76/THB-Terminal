/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: EnumDescription.cs,v 1.3 2010/11/24 16:04:10 kzmi Exp $
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Resources;
using System.Reflection;
using System.Diagnostics;

//namespace Poderosa.Util
namespace Telnet
{
    /// <summary>
    /// <ja>
    /// ƒJƒ‹ƒ`ƒƒî•ñ‚ðŽ¦‚·ƒIƒuƒWƒFƒNƒg‚Å‚·B
    /// </ja>
    /// <en>
    /// Object that shows culture information.
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// ‚±‚ÌƒNƒ‰ƒX‚Ì‰ðà‚ÍA‚Ü‚¾‚ ‚è‚Ü‚¹‚ñB
    /// </ja>
    /// <en>
    /// This class has not explained yet. 
    /// </en>
    /// </remarks>
    public class StringResource
    {
        private string _resourceName;
        private ResourceManager _resMan;
        private Assembly _asm;

        public StringResource(string name, Assembly asm)
        {
            Init(name, asm, true);
        }
        public StringResource(string name, Assembly asm, bool register_enumdesc)
        {
            Init(name, asm, register_enumdesc);
        }
        private void Init(string name, Assembly asm, bool register_enumdesc)
        {
            _resourceName = name;
            _asm = asm;
            LoadResourceManager();
            if (register_enumdesc) EnumDescAttribute.AddResourceTable(asm, this);
        }

        public string GetString(string id)
        {
            return _resMan.GetString(id); //‚à‚µ‚±‚ê‚ª’x‚¢‚æ‚¤‚È‚ç‚±‚ÌƒNƒ‰ƒX‚ÅƒLƒƒƒbƒVƒ…‚Å‚à‚Â‚­‚ê‚Î‚¢‚¢‚¾‚ë‚¤
        }

        private void LoadResourceManager()
        {
            CultureInfo ci = System.Threading.Thread.CurrentThread.CurrentUICulture;
            OnCultureChanged(ci); //‚±‚ê‚ÅResourceManager‚ðƒŠƒtƒŒƒbƒVƒ…
        }

        public void OnCultureChanged(CultureInfo newculture)
        {
            //“––Ê‚Í‰pŒêE“ú–{Œê‚µ‚©‚µ‚È‚¢
            if (newculture.Name.StartsWith("ja"))
                _resMan = new ResourceManager(_resourceName + "_ja", _asm);
            else
                _resMan = new ResourceManager(_resourceName, _asm);
        }
    }

	//®”‚Ìenum’l‚É•\‹L‚ð‚Â‚¯‚½‚è‘ŠŒÝ•ÏŠ·‚µ‚½‚è‚·‚é@\‘¢ã
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
	[AttributeUsage(AttributeTargets.Enum)]
	public class EnumDescAttribute : Attribute {

        private static Dictionary<Assembly, List<StringResource>> _assemblyToResource = new Dictionary<Assembly, List<StringResource>>();
		//•¶Žš—ñƒŠƒ\[ƒX‚ðŽg‚¤‚â‚Â‚Í‚±‚ê‚ª•K—v
        public static void AddResourceTable(Assembly asm, StringResource res) {
            if(_assemblyToResource.ContainsKey(asm))
                _assemblyToResource[asm].Add(res);
            else {
                List<StringResource> l = new List<StringResource>();
                l.Add(res);
                _assemblyToResource.Add(asm, l);
            }
		}
        public static void RemoveResourceTable(Assembly asm, StringResource res) {
            Debug.Assert(_assemblyToResource.ContainsKey(asm));
            List<StringResource> l  = _assemblyToResource[asm];
            l.Remove(res);

            //ƒGƒ“ƒgƒŠ‚Ìíœ‚Ü‚Å‚Í‚¢‚¢‚â
        }

        private Assembly _assembly;
		private string[] _descriptions;
		private Hashtable _descToValue;
		private string[] _names;
		private Hashtable _nameToValue;
		private StringResource _strResource;

		public EnumDescAttribute(Type t) {
			Init(t);
        }

		public void Init(Type t) {
            _strResource = null;
            _assembly = t.Assembly;

			MemberInfo[] ms = t.GetMembers();
			_descToValue = new Hashtable(ms.Length);
			_nameToValue = new Hashtable(ms.Length);

			ArrayList descriptions = new ArrayList(ms.Length);
			ArrayList names = new ArrayList(ms.Length);

			int expected = 0;
			foreach(MemberInfo mi in ms) {
				FieldInfo fi = mi as FieldInfo;
				if(fi!=null && fi.IsStatic && fi.IsPublic) {
					int intVal = (int)fi.GetValue(null); //intˆÈŠO‚ðƒx[ƒX‚É‚µ‚Ä‚¢‚éEnum’l‚ÍƒTƒ|[ƒgŠO
					if(intVal!=expected) throw new Exception("unexpected enum value order");
					EnumValueAttribute a = (EnumValueAttribute)(fi.GetCustomAttributes(typeof(EnumValueAttribute), false)[0]);
				
					string desc = a.Description;
					descriptions.Add(desc);
					_descToValue[desc] = intVal;

					string name = fi.Name;
					names.Add(name);
					_nameToValue[name] = intVal;

					expected++;
				}
			}

			_descriptions = (string[])descriptions.ToArray(typeof(string));
			_names        = (string[])names.ToArray(typeof(string));
		}

		public virtual string GetDescription(ValueType i) {
			return LoadString(_descriptions[(int)i]);
		}
		public virtual string GetRawDescription(ValueType i)
		{
			return _descriptions[(int)i];
		}
		public virtual ValueType FromDescription(string v, ValueType d)
		{
			if(v==null) return d;
			IDictionaryEnumerator ie = _descToValue.GetEnumerator();
			while(ie.MoveNext()) {
				if(v==LoadString((string)ie.Key)) return (ValueType)ie.Value;
			}
			return d;
		}
		public virtual string GetName(ValueType i) {
			return _names[(int)i];
		}
		public virtual ValueType FromName(string v) {
			return (ValueType)_nameToValue[v];
		}
		public virtual ValueType FromName(string v, ValueType d) {
			if(v==null) return d;
			ValueType t = (ValueType)_nameToValue[v];
			return t==null? d : t;
		}

		public virtual string[] DescriptionCollection() {
			string[] r = new string[_descriptions.Length];
			for(int i=0; i<r.Length; i++)
				r[i] = LoadString(_descriptions[i]);
			return r;
		}
		private string LoadString(string id) {
            if(_strResource==null) ResolveStringResource(id);
			string t = _strResource.GetString(id);
			return t==null? id : t;
		}
        private void ResolveStringResource(string id) {
            List<StringResource> l = _assemblyToResource[_assembly];
            foreach(StringResource res in l) {
                if(res.GetString(id)!=null) { //Œ©‚Â‚©‚ê‚ÎÌ—p
                    _strResource = res;
                    return;
                }
            }
            throw new Exception("String resource not found for " + id);
        }


		//ƒAƒgƒŠƒrƒ…[ƒg‚ðŽæ“¾‚·‚é
		private static Hashtable _typeToAttr = new Hashtable();
		public static EnumDescAttribute For(Type type) {
			EnumDescAttribute a = _typeToAttr[type] as EnumDescAttribute;
			if(a==null) {
				a = (EnumDescAttribute)(type.GetCustomAttributes(typeof(EnumDescAttribute), false)[0]);
				_typeToAttr.Add(type, a);
			}
			return a;
		}

	}

    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
	[AttributeUsage(AttributeTargets.Field)]
	public class EnumValueAttribute : Attribute {
		private string _description;

		public string Description {
			get {
				return _description;
			}
			set {
				_description = value;
			}
		}
	}

}