using System;
using NUnit.Framework;
using Playtika.Controllers;

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
        public void ControllerCompositeDisposable_OneDisposableThrows_ThrowsSingleExceptionAndDisposesOthers()
        {
            // Arrange
            var composite = new ControllerCompositeDisposable();
            var throwingDisposable = new TestDisposable { ThrowOnDispose = true };
            var normalDisposable = new TestDisposable();

            composite.Add(throwingDisposable);
            composite.Add(normalDisposable);

            // Act
            var exception = Assert.Throws<TestControllersException>(
                () => composite.Dispose(),
                "Expected TestControllersException from throwing disposable");

            // Assert
            Assert.IsNotNull(exception, "Exception should not be null");
            Assert.AreEqual(1, throwingDisposable.DisposeCallCount, "Throwing disposable should be disposed exactly once");
            Assert.AreEqual(1, normalDisposable.DisposeCallCount, "Normal disposable should still be disposed exactly once");
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

            // Act
            var aggregate = Assert.Throws<AggregateException>(
                () => composite.Dispose(),
                "Expected AggregateException when multiple disposables throw");

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

            // Act
            var aggregate = Assert.Throws<AggregateException>(
                () => composite.AddRange(collection),
                "Expected AggregateException when multiple disposables added after dispose throw");

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
