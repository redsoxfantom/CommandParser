using System;
using NUnit.Framework;
using CommandParser;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CommandParserTest
{
	[TestFixture]
	public class CommandParserTest
	{
		private bool delegateCalled;

		[TestFixtureSetUp]
		public void Init()
		{
			delegateCalled = false;
		}

		[Test]
		public void TestProcessNoCommandFound()
		{
			Parser target = new Parser (new Dictionary<Regex,Tuple<Type,Action<object>>>());
			Assert.IsFalse (target.Parse ("TestString"));
		}

		[Test]
		public void TestProcessCommandNoParams()
		{
			var dict = new Dictionary<Regex,Tuple<Type,Action<object>>> ();
			dict.Add (new Regex ("[Cc]ommand"), 
			          new Tuple<Type, Action<object>> (
				          typeof(EmptyParam), (param) =>
			{
				delegateCalled = true;
				Assert.IsInstanceOf<EmptyParam>(param);
			}));
			Parser target = new Parser (dict);

			Assert.IsTrue (target.Parse ("Command"));
			Assert.IsTrue (delegateCalled);

			delegateCalled = false;
			Assert.IsTrue (target.Parse ("command abcd"));
			Assert.IsTrue (delegateCalled);
		}

		[Test]
		public void TestProcessBooleanParam()
		{
			bool expectedValue = false;
			var dict = new Dictionary<Regex,Tuple<Type,Action<object>>> ();
			dict.Add (new Regex ("[Cc]ommand"), 
			          new Tuple<Type, Action<object>> (
				          typeof(OneBoolParam), (param) =>
			{
				delegateCalled = true;
				Assert.IsInstanceOf<OneBoolParam>(param);
				Assert.AreEqual(expectedValue,((OneBoolParam)param).Val);
			}));
			Parser target = new Parser (dict);

			expectedValue = false;
			Assert.IsTrue (target.Parse ("Command"));
			Assert.IsTrue (delegateCalled);

			expectedValue = true;
			delegateCalled = false;
			Assert.IsTrue (target.Parse ("command abcd Val"));
			Assert.IsTrue (delegateCalled);
		}

		[Test]
		public void TestProcessStringParam()
		{
			string expectedValue = null;
			var dict = new Dictionary<Regex,Tuple<Type,Action<object>>> ();
			dict.Add (new Regex ("[Cc]ommand"), 
			          new Tuple<Type, Action<object>> (
				          typeof(OneStringParam), (param) =>
			{
				delegateCalled = true;
				Assert.IsInstanceOf<OneStringParam>(param);
				Assert.AreEqual(expectedValue,((OneStringParam)param).Val);
			}));
			Parser target = new Parser (dict);

			expectedValue = null;
			Assert.IsTrue (target.Parse ("Command"));
			Assert.IsTrue (delegateCalled);

			expectedValue = "abcd";
			delegateCalled = false;
			Assert.IsTrue (target.Parse ("command Val abcd"));
			Assert.IsTrue (delegateCalled);
		}

		[Test]
		public void TestMultipleParams()
		{
			string expectedString = null;
			int expectedInt = 0;
			float expectedFloat = 0.0f;
			bool expectedBool = false;

			var dict = new Dictionary<Regex,Tuple<Type,Action<object>>> ();
			dict.Add (new Regex ("[Cc]ommand"), 
			          new Tuple<Type, Action<object>> (
				          typeof(MultipleParams), (param) =>
			{
				delegateCalled = true;
				Assert.IsInstanceOf<MultipleParams>(param);
				Assert.AreEqual(expectedString,((MultipleParams)param).strVal);
				Assert.AreEqual(expectedBool,((MultipleParams)param).boolVal);
				Assert.AreEqual(expectedInt,((MultipleParams)param).intVal);
				Assert.AreEqual(expectedFloat,((MultipleParams)param).floatVal);
			}));
			Parser target = new Parser (dict);

			Assert.IsTrue (target.Parse ("Command"));
			Assert.IsTrue (delegateCalled);

			expectedString = "abcd";
			expectedInt = 10;
			expectedFloat = 5.5f;
			expectedBool = true;
			delegateCalled = false;
			Assert.IsTrue (target.Parse ("command boolVal intVal 10 strVal abcd floatVal 5.5"));
			Assert.IsTrue (delegateCalled);

			expectedString = null;
			expectedInt = 0;
			expectedFloat = 5.5f;
			expectedBool = true;
			delegateCalled = false;
			Assert.IsTrue (target.Parse ("command boolVal floatVal 5.5"));
			Assert.IsTrue (delegateCalled);
		}

		[Test]
		public void TestValueParam()
		{
			string expectedValue = "Val";
			var dict = new Dictionary<Regex,Tuple<Type,Action<object>>> ();
			dict.Add (new Regex ("[Cc]ommand"), 
			          new Tuple<Type, Action<object>> (
				          typeof(ValueParam), (param) =>
			{
				delegateCalled = true;
				Assert.AreEqual(expectedValue,((ValueParam)param).Value);
			}));
			Parser target = new Parser (dict);

			target.Parse ("Command Val");
			Assert.IsTrue (delegateCalled);
		}

		[Test]
		public void TestAttrParam()
		{
			string expectedValue = "SomeVal";
			var dict = new Dictionary<Regex,Tuple<Type,Action<object>>> ();
			dict.Add (new Regex ("[Cc]ommand"), 
			          new Tuple<Type, Action<object>> (
				          typeof(AttributeParams), (param) =>
			{
				delegateCalled = true;
				Assert.AreEqual(expectedValue,((AttributeParams)param).Val);
			}));
			Parser target = new Parser (dict);

			target.Parse ("Command --Value SomeVal");
			Assert.IsTrue (delegateCalled);

			delegateCalled = false;
			target.Parse ("Command ThisIsAValue SomeVal");
			Assert.IsTrue (delegateCalled);
		}
	}

	public class EmptyParam	{}

	public class OneBoolParam
	{
		public bool Val {get;set;}
	}

	public class OneStringParam
	{
		public string Val {get;set;}
	}

	public class ValueParam
	{
		[Parameter(true)]
		public string Value{get;set;}
	}

	public class AttributeParams
	{
		[Parameter(false,"--Value","ThisIsAValue")]
		public string Val{get;set;}
	}

	public class MultipleParams
	{
		public bool boolVal {get;set;}
		public int intVal {get;set;}
		public float floatVal {get;set;}
		public string strVal {get;set;}
	}
}

