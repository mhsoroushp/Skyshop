namespace Infrastructure.Tests.Data;

using Core.Entities;
using Core.Interfaces;
using FluentAssertions;
using Infrastructure.Data;
using Moq;
using Xunit;

public class PaymentServiceTests
{
	[Fact]
	public async Task CreatePaymentAsync_Should_Return_Existing_Payment_When_Order_Already_Has_Payment()
	{
		// Arrange
		var repoMock = new Mock<IPaymentRepository>();
		var orderId = Guid.NewGuid();
		var existingPayment = new Payment
		{
			Id = Guid.NewGuid(),
			OrderId = orderId,
			Amount = 99.9m
		};

		repoMock.Setup(r => r.GetByOrderIdAsync(orderId))
			.ReturnsAsync(existingPayment);

		var service = new PaymentService(repoMock.Object);

		// Act
		var result = await service.CreatePaymentAsync(orderId, 120m);

		// Assert
		result.Should().NotBeNull();
		result.Should().BeSameAs(existingPayment);

		repoMock.Verify(r => r.GetByOrderIdAsync(orderId), Times.Once);
		repoMock.Verify(r => r.AddAsync(It.IsAny<Payment>()), Times.Never);
	}

	[Fact]
	public async Task CreatePaymentAsync_Should_Create_New_Pending_Payment_When_No_Existing_Payment()
	{
		// Arrange
		var repoMock = new Mock<IPaymentRepository>();
		var orderId = Guid.NewGuid();
		var amount = 120.5m;

		repoMock.Setup(r => r.GetByOrderIdAsync(orderId))
			.ReturnsAsync((Payment?)null);

		Payment? addedPayment = null;
		repoMock.Setup(r => r.AddAsync(It.IsAny<Payment>()))
			.Callback<Payment>(p => addedPayment = p)
			.Returns(Task.CompletedTask);

		var service = new PaymentService(repoMock.Object);
		var before = DateTime.UtcNow;

		// Act
		var result = await service.CreatePaymentAsync(orderId, amount);
		var after = DateTime.UtcNow;

		// Assert
		result.Should().NotBeNull();
		result.OrderId.Should().Be(orderId);
		result.Amount.Should().Be(amount);
		result.Status.Should().Be(Core.Enums.PaymentStatus.Pending);
		result.CreatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);

		addedPayment.Should().NotBeNull();
		addedPayment!.OrderId.Should().Be(orderId);
		addedPayment.Amount.Should().Be(amount);
		addedPayment.Status.Should().Be(Core.Enums.PaymentStatus.Pending);

		repoMock.Verify(r => r.GetByOrderIdAsync(orderId), Times.Once);
		repoMock.Verify(r => r.AddAsync(It.IsAny<Payment>()), Times.Once);
	}

    [Fact]
	public async Task UpdatePaymentStatusAsync_Should_Update_Payment_Status_When_Payment_PaymentId_Exist()
	{
		// Arrange
		var repoMock = new Mock<IPaymentRepository>();
		var paymentId = Guid.NewGuid();
		var status = Core.Enums.PaymentStatus.Succeeded;
		var transactionId = "txn_123";

		var existingPayment = new Payment
		{
			Id = paymentId,
			OrderId = Guid.NewGuid(),
			Amount = 100m,
			Status = Core.Enums.PaymentStatus.Pending
		};

		repoMock.Setup(r => r.GetByIdAsync(paymentId))
			.ReturnsAsync(existingPayment);

        Payment? updatedPayment = null;
		repoMock.Setup(r => r.UpdateAsync(existingPayment))
            .Callback<Payment>(p => updatedPayment = p)
			.Returns(Task.CompletedTask);

		var service = new PaymentService(repoMock.Object);
        var before = DateTime.UtcNow;

		// Act & Assert
		await service.UpdatePaymentStatusAsync(paymentId, status, transactionId);

		updatedPayment.Should().NotBeNull();
		updatedPayment!.Status.Should().Be(status);
		updatedPayment.TransactionId.Should().Be(transactionId);
		updatedPayment.ProcessedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(DateTime.UtcNow);

		repoMock.Verify(r => r.GetByIdAsync(paymentId), Times.Once);
		repoMock.Verify(r => r.UpdateAsync(It.IsAny<Payment>()), Times.Once);
	}

    [Fact]
    public async Task UpdatePaymentErrorAsync_Should_Update_Payment_Error_When_Payment_PaymentId_Exist(){

		var repoMock = new Mock<IPaymentRepository>();
		var paymentId = Guid.NewGuid();
		var errorMessage = "Some error occurred";

		var existingPayment = new Payment
		{
			Id = paymentId,
			OrderId = Guid.NewGuid(),
			Amount = 100m,
			Status = Core.Enums.PaymentStatus.Pending
		};

		repoMock.Setup(r => r.GetByIdAsync(paymentId))
			.ReturnsAsync(existingPayment);

        Payment? updatedPayment = null;
		repoMock.Setup(r => r.UpdateAsync(existingPayment))
            .Callback<Payment>(p => updatedPayment = p)
			.Returns(Task.CompletedTask);

		var service = new PaymentService(repoMock.Object);

		// Act
		await service.UpdatePaymentErrorAsync(paymentId, errorMessage);

		// Assert
		updatedPayment.Should().NotBeNull();
		updatedPayment!.ErrorMessage.Should().Be(errorMessage);
        updatedPayment!.Status.Should().Be(Core.Enums.PaymentStatus.Failed);

		repoMock.Verify(r => r.GetByIdAsync(paymentId), Times.Once);
		repoMock.Verify(r => r.UpdateAsync(It.IsAny<Payment>()), Times.Once);
    }

}