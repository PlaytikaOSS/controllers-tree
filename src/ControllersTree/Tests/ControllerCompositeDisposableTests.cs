using System;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Playtika.Controllers;
using UnityEngine;
using UnityEngine.TestTools;

namespace UnitTests.Controllers
{
    [TestFixture]
    public class ControllerCompositeDisposableTests
    {
        [Test]
        public void ControllerCompositeDisposable_SingleDisposable_Disposes()
        {
            // Arrange
            var composite = new ControllerCompositeDisposable();
            var disposable = new TestDisposable();

            composite.Add(disposable);

            // Act
            composite.Dispose();

            // Assert
            Assert.AreEqual(1, disposable.DisposeCallCount, "Disposable should be disposed exactly once");
        }

        [Test]
        public void ControllerCompositeDisposable_DisposeEmpty_DoesNotThrow()
        {
            // Arrange
            var composite = new ControllerCompositeDisposable();

            // Act / Assert
            Assert.DoesNotThrow(
                () =>
                {
                    composite.Dispose();
                });
        }

        [Test]
        public void ControllerCompositeDisposable_DisposeTwice_IsIdempotent()
        {
            // Arrange
            var composite = new ControllerCompositeDisposable();
            var disposable = new TestDisposable();
            composite.Add(disposable);

            // Act
            composite.Dispose();
            composite.Dispose();

            // Assert
            Assert.AreEqual(1, disposable.DisposeCallCount, "Disposable should still be disposed exactly once");
        }

        [Test]
        public void ControllerCompositeDisposable_AddAfterDisposed_DisposesImmediately()
        {
            // Arrange
            var composite = new ControllerCompositeDisposable();
            var disposable = new TestDisposable();

            composite.Dispose();

            // Act
            composite.Add(disposable);

            // Assert
            Assert.AreEqual(1, disposable.DisposeCallCount, "Disposable added after composite dispose should be disposed immediately");
        }

        [Test]
        public void ControllerCompositeDisposable_AddNull_IgnoredAndOtherDisposablesWork()
        {
            // Arrange
            var composite = new ControllerCompositeDisposable();
            var disposable = new TestDisposable();

            composite.Add(null);
            composite.Add(disposable);
            composite.Add(null);

            // Act
            composite.Dispose();

            // Assert
            Assert.AreEqual(1, disposable.DisposeCallCount, "Non-null disposable should be disposed exactly once");
        }

        [Test]
        public void ControllerCompositeDisposable_AddRangeBeforeDispose_DisposesAll()
        {
            // Arrange
            var composite = new ControllerCompositeDisposable();
            var d1 = new TestDisposable();
            var d2 = new TestDisposable();
            var collection = new[] { d1, d2 };

            composite.AddRange(collection);

            // Act
            composite.Dispose();

            // Assert
            Assert.AreEqual(1, d1.DisposeCallCount, "First disposable should be disposed exactly once");
            Assert.AreEqual(1, d2.DisposeCallCount, "Second disposable should be disposed exactly once");
        }

        [Test]
        public void ControllerCompositeDisposable_AddRangeNullCollection_DoesNotThrow()
        {
            // Arrange
            var composite = new ControllerCompositeDisposable();

            // Act / Assert
            Assert.DoesNotThrow(
                () =>
                {
                    composite.AddRange(null);
                    composite.Dispose();
                });
        }

        [Test]
        public void ControllerCompositeDisposable_AddRangeWithNullItems_DisposesNonNullOnly()
        {
            // Arrange
            var composite = new ControllerCompositeDisposable();
            var d1 = new TestDisposable();
            var d3 = new TestDisposable();
            var collection = new[] { d1, null, d3 };

            composite.AddRange(collection);

            // Act
            composite.Dispose();

            // Assert
            Assert.AreEqual(1, d1.DisposeCallCount, "First non-null disposable should be disposed exactly once");
            Assert.AreEqual(1, d3.DisposeCallCount, "Second non-null disposable should be disposed exactly once");
        }

        [Test]
        public void ControllerCompositeDisposable_AddRangeAfterDisposed_DisposesAllImmediately()
        {
            // Arrange
            var composite = new ControllerCompositeDisposable();
            var d1 = new TestDisposable();
            var d2 = new TestDisposable();
            var collection = new[] { d1, d2 };

            composite.Dispose();

            // Act
            composite.AddRange(collection);

            // Assert
            Assert.AreEqual(1, d1.DisposeCallCount, "First disposable should be disposed immediately");
            Assert.AreEqual(1, d2.DisposeCallCount, "Second disposable should be disposed immediately");
        }

        [Test]
        public void ControllerCompositeDisposable_MultipleDisposablesThrow_ThrowsAggregateExceptionWithAllInner()
        {
            // Arrange
            var composite = new ControllerCompositeDisposable();
            var d1 = new TestDisposable("d1") { ThrowOnDispose = true };
            var d2 = new TestDisposable("d2") { ThrowOnDispose = true };
            var d3 = new TestDisposable("d3");

            composite.Add(d1);
            composite.Add(d2);
            composite.Add(d3);
            
            LogAssert.Expect(LogType.Exception, new Regex("TestControllersException"));
            LogAssert.Expect(LogType.Exception, new Regex("TestControllersException"));

            // Act
            var aggregate = Assert.Throws<ControllerDisposeAggregateException>(
                () => composite.Dispose(),
                "Expected ControllerDisposeAggregateException when multiple disposables throw");

            // Assert
            Assert.IsNotNull(aggregate, "AggregateException should not be null");
            Assert.AreEqual(2, aggregate.InnerExceptions.Count, "AggregateException should contain all throwing disposables");
            foreach (var inner in aggregate.InnerExceptions)
            {
                Assert.IsInstanceOf<TestControllersException>(inner, "Inner exception should be TestControllersException");
            }

            Assert.AreEqual(1, d1.DisposeCallCount, "First throwing disposable should be disposed exactly once");
            Assert.AreEqual(1, d2.DisposeCallCount, "Second throwing disposable should be disposed exactly once");
            Assert.AreEqual(1, d3.DisposeCallCount, "Non-throwing disposable should be disposed exactly once");
        }

        [Test]
        public void ControllerCompositeDisposable_AddRangeAfterDisposed_MultipleDisposablesThrow_ThrowsAggregateException()
        {
            // Arrange
            var composite = new ControllerCompositeDisposable();
            var d1 = new TestDisposable("d1") { ThrowOnDispose = true };
            var d2 = new TestDisposable("d2") { ThrowOnDispose = true };
            var d3 = new TestDisposable("d3"); // does not throw
            var collection = new[] { d1, d2, d3 };

            composite.Dispose();
            LogAssert.Expect(LogType.Exception, new Regex("TestControllersException"));
            LogAssert.Expect(LogType.Exception, new Regex("TestControllersException"));

            // Act
            var aggregate = Assert.Throws<ControllerDisposeAggregateException>(
                () => composite.AddRange(collection),
                "Expected ControllerDisposeAggregateException when multiple disposables added after dispose throw");

            // Assert
            Assert.IsNotNull(aggregate, "AggregateException should not be null");
            Assert.AreEqual(2, aggregate.InnerExceptions.Count, "AggregateException should contain all throwing disposables");
            foreach (var inner in aggregate.InnerExceptions)
            {
                Assert.IsInstanceOf<TestControllersException>(inner, "Inner exception should be TestControllersException");
            }

            Assert.AreEqual(1, d1.DisposeCallCount, "First throwing disposable should be disposed exactly once");
            Assert.AreEqual(1, d2.DisposeCallCount, "Second throwing disposable should be disposed exactly once");
            Assert.AreEqual(1, d3.DisposeCallCount, "Non-throwing disposable should be disposed exactly once");
        }

        [Test]
        public void ControllerCompositeDisposable_NestedComposite_InnerThrows_LogsLeafOnce_OuterDoesNotLogAggregate()
        {
            // Arrange
            var outer = new ControllerCompositeDisposable();
            var inner = new ControllerCompositeDisposable();

            var innerThrowing = new TestDisposable("inner") { ThrowOnDispose = true };
            var outerNonThrowing = new TestDisposable("outer-non-throwing");

            inner.Add(innerThrowing);
            outer.Add(inner);
            outer.Add(outerNonThrowing);

            // Expect exactly one leaf exception log. Aggregate must not be logged.
            LogAssert.Expect(LogType.Exception, new Regex("TestControllersException"));

            // Act
            var aggregate = Assert.Throws<ControllerDisposeAggregateException>(
                () => outer.Dispose(),
                "Expected ControllerDisposeAggregateException from outer composite");

            // Assert
            Assert.NotNull(aggregate);
            Assert.AreEqual(1, aggregate.InnerExceptions.Count);
            Assert.IsInstanceOf<TestControllersException>(aggregate.InnerExceptions[0]);

            Assert.AreEqual(1, innerThrowing.DisposeCallCount);
            Assert.AreEqual(1, outerNonThrowing.DisposeCallCount);

            // Ensures no extra exception logs happened (e.g. aggregate logged by outer).
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void ControllerCompositeDisposable_NestedComposite_MultipleLeafThrows_EachLeafLoggedOnce()
        {
            // Arrange
            var outer = new ControllerCompositeDisposable();
            var mid = new ControllerCompositeDisposable();
            var inner = new ControllerCompositeDisposable();

            var d1 = new TestDisposable("d1-inner") { ThrowOnDispose = true };
            var d2 = new TestDisposable("d2-mid") { ThrowOnDispose = true };
            var d3 = new TestDisposable("d3-outer") { ThrowOnDispose = true };
            var d4 = new TestDisposable("d4-outer-non-throwing");

            inner.Add(d1);
            mid.Add(inner);
            mid.Add(d2);

            outer.Add(mid);
            outer.Add(d3);
            outer.Add(d4);

            // Expect exactly 3 leaf exception logs total; aggregates must not be logged.
            LogAssert.Expect(LogType.Exception, new Regex("TestControllersException"));
            LogAssert.Expect(LogType.Exception, new Regex("TestControllersException"));
            LogAssert.Expect(LogType.Exception, new Regex("TestControllersException"));

            // Act
            var aggregate = Assert.Throws<ControllerDisposeAggregateException>(() => outer.Dispose());

            // Assert
            Assert.NotNull(aggregate);
            Assert.AreEqual(3, aggregate.InnerExceptions.Count);
            foreach (var ex in aggregate.InnerExceptions)
            {
                Assert.IsInstanceOf<TestControllersException>(ex);
            }

            Assert.AreEqual(1, d1.DisposeCallCount);
            Assert.AreEqual(1, d2.DisposeCallCount);
            Assert.AreEqual(1, d3.DisposeCallCount);
            Assert.AreEqual(1, d4.DisposeCallCount);

            LogAssert.NoUnexpectedReceived();
        }

        private sealed class TestDisposable : IDisposable
        {
            public int DisposeCallCount { get; private set; }
            public bool ThrowOnDispose { get; set; }
            public string Id { get; }

            public TestDisposable(string id = null)
            {
                Id = id;
            }

            public void Dispose()
            {
                DisposeCallCount++;

                if (ThrowOnDispose)
                {
                    throw new TestControllersException($"Dispose:{Id ?? "unknown"}");
                }
            }
        }
    }
}
