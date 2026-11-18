public sealed class SagaCoordinator(SagaJournal journal, HttpClient inventory, HttpClient payments)
{
    public async Task RunAsync(bool interruptAfterReservation = false)
    {
        if (journal.State == "Started")
        {
            using var reservation = await inventory.PostAsync("/reserve/enroll-11", null);
            reservation.EnsureSuccessStatusCode();
            if (interruptAfterReservation) throw new InvalidOperationException("Simulated interruption after remote reservation");
            journal.Move("Started", "Reserved");
        }
        if (journal.State == "Reserved")
        {
            using var payment = await payments.PostAsync("/charge/enroll-11", null);
            if (payment.IsSuccessStatusCode) journal.Move("Reserved", "Completed");
            else if (payment.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity)
                journal.Move("Reserved", "Compensating");
            else throw new HttpRequestException("Payment outcome requires reconciliation; saga remains Reserved");
        }
        if (journal.State == "Compensating")
        {
            using var released = await inventory.PostAsync("/release/enroll-11", null);
            released.EnsureSuccessStatusCode();
            journal.Move("Compensating", "Compensated");
        }
    }
}
