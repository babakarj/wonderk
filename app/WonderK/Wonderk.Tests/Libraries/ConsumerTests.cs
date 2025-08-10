using Microsoft.Extensions.Logging;
using Moq;
using WonderK.Common.Data;
using WonderK.Common.Libraries;

namespace Wonderk.Tests.Libraries
{
    public class ConsumerTests
    {
        public class SimpleConsumer(IQueueProcessor queue, IProcessLogger processLogger, ILogger<Consumer> logger)
            : Consumer(queue, processLogger, logger)
        {
            public override Task Process(Package package) => base.Process(package);
        }

        public class ComplexConsumer(IQueueProcessor queue, IProcessLogger processLogger, ILogger<Consumer> logger)
            : Consumer(queue, processLogger, logger)
        {
            public override Task Process(Package package)
            {
                base.Process(package);
                package.Metadata.AddLast($"The package has been signed.");
                return Task.CompletedTask;
            }
        }

        [Test]
        public async Task Listen_CallsProcessAndForward()
        {
            var queueMock = new Mock<IQueueProcessor>();
            var loggerMock = new Mock<IProcessLogger>();
            var consumerLoggerMock = new Mock<ILogger<Consumer>>();
            var packageJson = "{\"Departments\":[\"A\",\"B\"]}";
            var processCalled = false;
            var forwardCalled = false;

            queueMock.Setup(q => q.Consume(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Action<string>>()))
                .Returns<string, string, string, Action<string>>((sk, gn, cn, action) =>
                {
                    action(packageJson);
                    return Task.CompletedTask;
                });

            var consumerSpy = new Mock<SimpleConsumer>(queueMock.Object, loggerMock.Object, consumerLoggerMock.Object)
            {
                CallBase = true
            };

            consumerSpy.Setup(c => c.Process(It.IsAny<Package>()))
                .Callback(() => processCalled = true)
                .Returns(Task.CompletedTask);

            consumerSpy.Setup(c => c.Forward(It.IsAny<Package>()))
                .Callback(() => forwardCalled = true)
                .Returns(Task.CompletedTask);

            await consumerSpy.Object.Listen("stream", "group", "consumer");

            Assert.IsTrue(processCalled);
            Assert.IsTrue(forwardCalled);
        }

        [Test]
        public async Task Process_RemovesFirstDepartment_WhenDepartmentsNotEmpty()
        {
            var queueMock = new Mock<IQueueProcessor>();
            var loggerMock = new Mock<IProcessLogger>();
            var consumerLoggerMock = new Mock<ILogger<Consumer>>();
            var departments = new LinkedList<string>(["Dept1", "Dept2"]);
            var package = new Package { Departments = departments };

            var consumer = new SimpleConsumer(queueMock.Object, loggerMock.Object, consumerLoggerMock.Object);
            await consumer.Process(package);

            Assert.IsFalse(package.Departments.Contains("Dept1"));
            Assert.IsTrue(package.Departments.Contains("Dept2"));
        }

        [Test]
        public async Task Process_DoesNothing_WhenDepartmentsEmpty()
        {
            var queueMock = new Mock<IQueueProcessor>();
            var loggerMock = new Mock<IProcessLogger>();
            var consumerLoggerMock = new Mock<ILogger<Consumer>>();
            var departments = new LinkedList<string>();
            var package = new Package { Departments = departments };

            var consumer = new SimpleConsumer(queueMock.Object, loggerMock.Object, consumerLoggerMock.Object);
            await consumer.Process(package);

            Assert.IsEmpty(package.Departments);
        }

        [Test]
        public async Task Forward_ProducesToNextConsumer_WhenDepartmentsNotEmpty()
        {
            var queueMock = new Mock<IQueueProcessor>();
            var loggerMock = new Mock<IProcessLogger>();
            var consumerLoggerMock = new Mock<ILogger<Consumer>>();
            var departments = new LinkedList<string>(["NextDepartment"]);
            var package = new Package { Departments = departments };
            var consumer = new SimpleConsumer(queueMock.Object, loggerMock.Object, consumerLoggerMock.Object);

            queueMock.Setup(q => q.Produce("NextDepartment-stream", package.ToString()))
                .ReturnsAsync("ok")
                .Verifiable();

            await consumer.Forward(package);

            queueMock.Verify(q => q.Produce("NextDepartment-stream", package.ToString()), Times.Once);
        }

        [Test]
        public async Task Forward_WritesNoMoreConsumers_WhenDepartmentsEmpty()
        {
            var queueMock = new Mock<IQueueProcessor>();
            var loggerMock = new Mock<IProcessLogger>();

            var loggerProvider = new TestLoggerProvider();
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Debug);
                builder.AddProvider(loggerProvider);
            });
            ILogger<Consumer> logger = loggerFactory.CreateLogger<Consumer>();

            var departments = new LinkedList<string>();
            var package = new Package { Departments = departments };
            var consumer = new SimpleConsumer(queueMock.Object, loggerMock.Object, logger);

            await consumer.Forward(package);

            var logMessages = loggerProvider.GetLogMessages();
            Assert.That(logMessages, Has.Count.EqualTo(1));
            Assert.IsTrue(logMessages.Any(m => m.Contains("No more consumers to forward the package to.")));
        }

        [Test]
        public async Task Forward_WritesConsumer_WhenDepartmentsNotEmpty()
        {
            var queueMock = new Mock<IQueueProcessor>();
            var loggerMock = new Mock<IProcessLogger>();

            var loggerProvider = new TestLoggerProvider();
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Debug);
                builder.AddProvider(loggerProvider);
            });
            ILogger<Consumer> logger = loggerFactory.CreateLogger<Consumer>();

            const string nextConsumer = "A";
            var departments = new LinkedList<string>([nextConsumer, "B"]);
            var package = new Package { Departments = departments };
            var consumer = new SimpleConsumer(queueMock.Object, loggerMock.Object, logger);

            await consumer.Forward(package);

            var logMessages = loggerProvider.GetLogMessages();
            Assert.That(logMessages, Has.Count.EqualTo(1));
            Assert.IsTrue(logMessages.Any(m => m.Contains($"Forwarding package to {nextConsumer}.")));
        }

        [Test]
        public async Task PackageMetadata_Should_Not_Change_In_SimpleConsumer()
        {
            var queueMock = new Mock<IQueueProcessor>();
            var loggerMock = new Mock<IProcessLogger>();
            var consumerLoggerMock = new Mock<ILogger<Consumer>>();
            var package = new Package
            {
                Id = "PKG-12345",
                Departments = new LinkedList<string>(["A", "B", "C"]),
                Parcel = new Parcel
                {
                    Receipient = new Receipient
                    {
                        Name = "John Smith",
                        Address = new Address
                        {
                            Street = "Main St",
                            HouseNumber = 42,
                            PostalCode = "1234AB",
                            City = "City1"
                        }
                    },
                    Weight = 2.5,
                    Value = 150.0
                }
            };
            var consumer = new SimpleConsumer(queueMock.Object, loggerMock.Object, consumerLoggerMock.Object);
            await consumer.Process(package);

            Assert.IsEmpty(package.Metadata);
        }

        [Test]
        public async Task PackageMetadata_Should_Change_In_ComplexConsumer()
        {
            var queueMock = new Mock<IQueueProcessor>();
            var loggerMock = new Mock<IProcessLogger>();
            var consumerLoggerMock = new Mock<ILogger<Consumer>>();
            var package = new Package
            {
                Id = "PKG-12345",
                Departments = new LinkedList<string>(["A", "B", "C"]),
                Parcel = new Parcel
                {
                    Receipient = new Receipient
                    {
                        Name = "John Smith",
                        Address = new Address
                        {
                            Street = "Main St",
                            HouseNumber = 42,
                            PostalCode = "1234AB",
                            City = "City1"
                        }
                    },
                    Weight = 2.5,
                    Value = 150.0
                }
            };
            var consumer = new ComplexConsumer(queueMock.Object, loggerMock.Object, consumerLoggerMock.Object);
            await consumer.Process(package);

            Assert.IsNotEmpty(package.Metadata);
            Assert.IsTrue(package.Metadata.Contains("The package has been signed."));
        }
    }
}