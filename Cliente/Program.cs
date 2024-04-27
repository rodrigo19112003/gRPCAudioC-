using Grpc.Core;
using Grpc.Net.Client;
using System.Media;
using static AudioService;

static async Task<MemoryStream> downloadsStreamAsync(AudioServiceClient stub, string fileName)
{
    using var call = stub.downloadAudio(new DownloadFileRequest
    {
        Name = fileName
    });

    Console.WriteLine($"Recibiendo el archivo: {fileName}");
    var writeStream = new MemoryStream();

    await foreach (var message in call.ResponseStream.ReadAllAsync())
    {
        if (message.Data != null)
        {
            var bytes = message.Data.Memory;
            Console.Write(".");
            await writeStream.WriteAsync(bytes);
        }
    }
    Console.WriteLine("\nRecepcion de datos correcta.\n\n");
    return writeStream;
}

static void playStream(MemoryStream stream, string fileName)
{
    if (stream != null)
    {
        Console.WriteLine($"Reproduciendo el archivo: {fileName}...\n\n");
        SoundPlayer soundPlayer = new(stream);
        soundPlayer.Stream?.Seek(0, SeekOrigin.Begin);
        soundPlayer.Play();
    }
}

using var channel = GrpcChannel.ForAddress("http://localhost:8080");
AudioServiceClient stub = new(channel);

string fileName = "anyma.wav";

MemoryStream stream = await downloadsStreamAsync(stub, fileName);
playStream(stream, fileName);

Console.WriteLine("Presione cualquier tecla para terminar el programa..."); Console.ReadKey();
stream.Close();
Console.WriteLine("Apagando...");
channel.ShutdownAsync().Wait();


