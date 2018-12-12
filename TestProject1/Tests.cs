using System;
using System.Collections.Generic;
using DependencyContainer;
using NUnit.Framework;

namespace TestProject1
{
    public interface Interf {}
	public class Class1 : Interf {}
	public class Class2 : Class1
	{
		public Class2 (C c)
		{

		}
	}
	public class B2 : Class1 { }
	public class C {}
	public class D : C {}
	public class G : B2 {}
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
		private DependencyConfig config;
		private DependencyProvider provider;

		[OneTimeSetUp]
		public void TestInitilize()
		{
			config = new DependencyConfig();
		}

		[Test]
		public void CorrectGenericCreationByValidator()
		{
			config.registrateClass(typeof(IGen1<>), typeof(Gen1<>));
			config.registrateClass(typeof(IGen2<>), typeof(Gen2<>));
			provider = new DependencyProvider(config);
			Assert.IsNotNull(provider.resolve<IGen1<int>>(), "Generic creation error");
		}

		[Test]
		public void CorrectCountOfCreatedObjectsByValidator()
		{
			int expected = 2;
			int actual;
			config.registrateClass<Class1,Class2>();
			config.registrateClass<Class2, B2>();
			config.registrateClass<C, D>();
			provider = new DependencyProvider(config);
			actual = ((List<Class1>)provider.resolveAll<Class1>()).Count;
			Assert.AreEqual(expected, actual, "Wrong types count");
		}

		[Test]
		public void CorrectTypeOfCreatedByProviderInstance()
		{
			config.registrateClass<Interf, Class1>();
			config.registrateClass<Class1, B2>();
			config.registrateClass<B2, G>();
			provider = new DependencyProvider(config);
			Assert.IsTrue(provider.resolve<Interf>() is G, "Wrong type");
		}

		[Test]
		public void CorrectTypeOfCreatedByProviderInstanceWithAsSelfMode()
		{
			config.registrateClass<Class1, Class1>();
			provider = new DependencyProvider(config);
			Assert.IsNotNull(provider.resolve<Class1>(), "Wrong type");
		}

		[Test]
		public void ProviderCantCreateWrongGeneric()
		{
			config.registrateClass(typeof(IGen2<>), typeof(Gen3<>));
			provider = new DependencyProvider(config);
			Assert.IsNull(provider.resolve<IGen2<string>>(), "Wrong generic created");
		}

		[Test]
		public void ProviderCreatesGenericWithSameAttr()
		{
			config.registrateClass(typeof(IGen2<>), typeof(Gen3<>));
			provider = new DependencyProvider(config);
			Assert.IsNotNull(provider.resolve<IGen2<int>>(), "Generic creation error");
		}

		[Test]
		public void ValidatorSpotsRecursion()
		{
			config.registrateClass<Rec1, Rec1>();
			config.registrateClass<Rec2, Rec2>();
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
		public void ProviderCreatesSinltoneObjects()
		{
			Interf I1, I2;
			config.registrateSingletoneClass<Interf, Class1>();
			config.registrateClass<Class1, B2>();
			config.registrateClass<B2, G>();
			provider = new DependencyProvider(config);
			I1 = provider.resolve<Interf>();
			I2 = provider.resolve<Interf>();
			Assert.AreSame(I1, I2, "Provider creates different objects");
		}

		[Test]
		public void CorrectGenericSingletonObjectCreation()
		{
			IGen1<int> I1, I2;
			config.registrateSingletoneClass(typeof(IGen1<>), typeof(Gen1<>));
			config.registrateSingletoneClass(typeof(IGen2<>), typeof(Gen2<>));
			provider = new DependencyProvider(config);
			I1 = provider.resolve<IGen1<int>>();
			I2 = provider.resolve<IGen1<int>>();
			Assert.AreSame(I1, I2, "Provider creates different objects");
		}
	}
}