var repository = new Repository<Invoice>();
repository.Add(new Invoice(7, 128.90m));
Check.That(repository.Find(7)?.Net == 128.90m, "constraint permits access to the entity identifier");
Check.That(repository.Find(99) is null, "missing entity remains explicit");

ISource<Invoice> invoiceSource = new SingleSource<Invoice>(new Invoice(7, 128.90m));
ISource<Document> documentSource = invoiceSource;
Check.That(documentSource.Read().Id == 7, "covariant source produces a usable base document");

var generalSink = new DocumentSink();
ISink<Invoice> invoiceSink = generalSink;
invoiceSink.Write(new Invoice(8, 10m));
Check.That(generalSink.LastId == 8, "contravariant document sink accepts an invoice");
