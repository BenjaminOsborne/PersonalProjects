var message = GetMessage(args);

static string GetMessage(string[] args) =>
    (args.Length > 0) ? string.Join(" ", args) : "Hello World!";