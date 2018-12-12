using System;
using System.Collections.Generic;
using DependencyContainer;
using NUnit.Framework;

namespace TestProject1
{
    public interface I {}
	public class A_I : I {}
	public class B_A_I : A_I
	{
		public B_A_I (C c)
		{

		}
	}
	public class B2_A_I : A_I { }
	public class C {}
	public class D_C : C {}
	public interface IGen1<T> { }
	public interface IGen2<T> { }
	public class Gen1<T> : IGen1<T>
	{
		public Gen1(IGen2<T> gen2)
		{

		}
	}
	public class Gen2<T> : IGen2<T> { }
	public class Gen3<T> : IGen2<int> { }

	public class Rec1 { public Rec1(Rec2 rec2) {} }
	public class Rec2 { public Rec2(Rec1 rec1) {} }


	[TestFixture]
	public class ProviderTests
	{
		private DependenciesConfiguration config;
		private DependencyProvider provider;

		[OneTimeSetUp]
		public void TestInitilize()
		{
			config = new DependenciesConfiguration();
		}

		[Test]
		public void CorrectGenericCreationByValidator()
		{
			config.Registrate(typeof(IGen1<>), typeof(Gen1<>));
			config.Registrate(typeof(IGen2<>), typeof(Gen2<>));
			provider = new DependencyProvider(config);
			Assert.IsNotNull(provider.Resolve<IGen1<int>>(), "Generic creation error");
		}

		[Test]
		public void CorrectCountOfCreatedObjectsByValidator()
		{
			int expected = 2;
			int actual;
			config.Registrate<A_I, B_A_I>();
			config.Registrate<A_I, B2_A_I>();
			config.Registrate<C, D_C>();
			provider = new DependencyProvider(config);
			actual = ((List<A_I>)provider.ResolveAll<A_I>()).Count;
			Assert.AreEqual(expected, actual, "Wrong types count");
		}

		[Test]
		public void correctGenericSingletonObjectCreation()
		{
			IGen1<int> I1, I2;
			config.RegistrateAsSingleton(typeof(IGen1<>), typeof(Gen1<>));
			config.RegistrateAsSingleton(typeof(IGen2<>), typeof(Gen2<>));
			provider = new DependencyProvider(config);
			I1 = provider.Resolve<IGen1<int>>();
			I2 = provider.Resolve<IGen1<int>>();
			Assert.AreSame(I1, I2, "Provider creates different objects");
		}
		
		[Test]
		public void genericDefinitionAndCreate()
		{
			config.Registrate<I, A_I>();
			provider = new DependencyProvider(config);
			Assert.IsTrue(provider.Resolve<I>() is A_I, "Wrong type");
		}

		[Test]
		public void createdByProviderInstanceWithAsSelfMode()
		{
			config.Registrate<A_I, A_I>();
			provider = new DependencyProvider(config);
			Assert.IsNotNull(provider.Resolve<A_I>() is A_I, "Wrong type");
		}

		[Test]
		public void ProviderCreatesGenericWithSameAttr()
		{
			config.Registrate(typeof(IGen2<>), typeof(Gen3<>));
			provider = new DependencyProvider(config);
			Assert.IsNotNull(provider.Resolve<IGen2<int>>(), "Generic creation error");
		}

		[Test]
		public void ValidatorSpotsRecursion()
		{
			config.Registrate<Rec1, Rec1>();
			config.Registrate<Rec2, Rec2>();
			try
			{
				new DependencyProvider(config);
				Assert.Fail("Validator confirm wrong config");
			}
			catch (ArgumentException e)
			{
			}
			catch (Exception e)
			{
				throw e;
			}
		}

		[Test]
		public void ProviderCreatesSingltoneObjects()
		{
			I I1, I2;
			config.RegistrateAsSingleton<I, A_I>();
			provider = new DependencyProvider(config);
			I1 = provider.Resolve<I>();
			I2 = provider.Resolve<I>();
			Assert.AreSame(I1, I2, "Provider creates different objects");
		}

	}
}