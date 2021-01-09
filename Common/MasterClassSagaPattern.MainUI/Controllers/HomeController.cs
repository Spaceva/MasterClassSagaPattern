using MassTransit;
using MasterClassSagaPattern.Messages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace MasterClassSagaPattern.MainUI.Controllers
{
    public class HomeController : Controller
    {
        private readonly MainDbContext dbContext;
        private readonly IPublishEndpoint publishEndpoint;
        private readonly ISendEndpointProvider sendEndpointProvider;

        public HomeController(MainDbContext dbContext, IPublishEndpoint publishEndpoint, ISendEndpointProvider sendEndpointProvider)
        {
            this.dbContext = dbContext;
            this.publishEndpoint = publishEndpoint;
            this.sendEndpointProvider = sendEndpointProvider;
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> Payments()
        {
            var payments = await dbContext.Payments.AsNoTracking().ToArrayAsync();

            ViewBag.Payments = payments;

            return View();
        }

        [HttpPost]
        public async Task<StatusCodeResult> AcceptPayment(Guid transactionId)
        {
            var transaction = await dbContext.Payments.FindAsync(transactionId);

            if (transaction is null)
            {
                return NotFound();
            }

            if (transaction.Status != Payment.PaymentStatus.Pending)
            {
                return BadRequest();
            }

            transaction.Status = Payment.PaymentStatus.Accepted;

            await dbContext.SaveChangesAsync();

            var sendEndpoint = await sendEndpointProvider.GetSendEndpoint(new Uri("queue:payment"));

            await sendEndpoint.Send<AcceptPayment>(new { CorrelationId = transactionId });

            return Ok();
        }

        [HttpPost]
        public async Task<StatusCodeResult> RefusePayment(Guid transactionId, string reason)
        {
            var transaction = await dbContext.Payments.FindAsync(transactionId);

            if (transaction is null)
            {
                return NotFound();
            }

            if (transaction.Status != Payment.PaymentStatus.Pending)
            {
                return BadRequest();
            }

            transaction.Status = Payment.PaymentStatus.Refused;

            await dbContext.SaveChangesAsync();

            var sendEndpoint = await sendEndpointProvider.GetSendEndpoint(new Uri("queue:payment"));

            await sendEndpoint.Send<RefusePayment>(new { CorrelationId = transactionId, Reason = reason });

            return Ok();
        }

        [HttpPost]
        public async Task<StatusCodeResult> OrderNew(int quantity)
        {
            var transactionId = Guid.NewGuid();

            dbContext.Payments.Add(new Payment { Id = transactionId });

            await dbContext.SaveChangesAsync();

            await publishEndpoint.Publish<OrderCreated>(new { CorrelationId = transactionId, Quantity = quantity, Address = "somewhere" });

            return Ok();
        }
    }
}
