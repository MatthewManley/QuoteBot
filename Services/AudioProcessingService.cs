using Domain.Models;
using Domain.Repositories;
using Domain.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Services
{
    public class AudioProcessingService : IAudioProcessingService
    {
        private readonly IAudioRepo audioRepo;
        private readonly IAudioOwnerRepo audioOwnerRepo;
        private const long MaxFileLength = 1024 * 1024 * 10;
        private readonly IReadOnlyList<string> permittedExtensions = new List<string>() { ".mp3", ".wav", ".opus", ".ogg", ".flac" };
        private readonly IReadOnlyList<string> permittedFormats = new List<string>() { "mp3", "wav", "ogg", "flac" };


        public AudioProcessingService(IAudioRepo audioRepo, IAudioOwnerRepo audioOwnerRepo)
        {
            this.audioRepo = audioRepo;
            this.audioOwnerRepo = audioOwnerRepo;
        }

        public async Task<AudioOwner> Upload(IFormFile formFile, CancellationToken token, ulong owner, ulong uploader, string name)
        {

            if (formFile.Length > MaxFileLength)
            {
                throw new Exception();
            }
            var extension = Path.GetExtension(formFile.FileName).ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(extension) || !permittedExtensions.Contains(extension))
            {
                throw new Exception();
            }

            var fileName = Guid.NewGuid().ToString();
            var tempPath = Path.GetTempPath();

            var uploadsDir = Path.Combine(tempPath, "Uploads");
            var intermediateDir = Path.Combine(tempPath, "Intermediate");
            var outputDir = Path.Combine(tempPath, "Processed");

            if (!Directory.Exists(uploadsDir))
                Directory.CreateDirectory(uploadsDir);
            if (!Directory.Exists(intermediateDir))
                Directory.CreateDirectory(intermediateDir);
            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            var filePath = Path.Combine(uploadsDir, fileName);
            var intermediatePath = Path.Combine(intermediateDir, fileName);
            var outputPath = Path.Combine(outputDir, fileName);

            try
            {

                using (var stream = File.Create(filePath))
                {
                    await formFile.CopyToAsync(stream, token);
                }
                if (token.IsCancellationRequested)
                    throw new Exception();

                var format = await GetFormat(filePath, token);
                if (string.IsNullOrWhiteSpace(format.DurationString) || format.Duration > 30)
                {
                    throw new Exception();
                }
                if (string.IsNullOrWhiteSpace(format.FormatName) || !permittedFormats.Contains(format.FormatName))
                {
                    throw new Exception();
                }

                var ffmpegArguments = $"-hide_banner -i \"{filePath}\" -ac 2 -ar 48000 {intermediatePath}.wav";
                var ffmpegProcessInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = ffmpegArguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                };

                using var ffmpegProcess = Process.Start(ffmpegProcessInfo);
                var ffmpegOutput = await ffmpegProcess.StandardOutput.ReadToEndAsync();
                var ffmpegError = await ffmpegProcess.StandardError.ReadToEndAsync();
                await ffmpegProcess.WaitForExitAsync(token);
                if (ffmpegProcess.ExitCode != 0 || token.IsCancellationRequested)
                    throw new Exception();

                var opusEncArguments = $"--bitrate 64 {intermediatePath}.wav \"{outputPath}\"";
                var opusEncProcessInfo = new ProcessStartInfo
                {
                    FileName = "opusenc",
                    Arguments = opusEncArguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using var opusEncProcess = Process.Start(opusEncProcessInfo);
                var opusEncOutput = await opusEncProcess.StandardOutput.ReadToEndAsync();
                var opusEncError = await opusEncProcess.StandardError.ReadToEndAsync();
                await opusEncProcess.WaitForExitAsync(token);
                if (opusEncProcess.ExitCode != 0 || token.IsCancellationRequested)
                    throw new Exception();

                await audioRepo.UploadFile(outputPath, fileName);
                var audioId = await audioRepo.CreateAudio(fileName, uploader);
                return await audioOwnerRepo.CreateAudioOwner(audioId.Id, owner, name);

            }
            //catch (Exception)
            //{
            //    throw;
            //}
            finally
            {
                try
                {
                    if (File.Exists(filePath))
                        File.Delete(filePath);
                }
                catch (Exception) { }
                try
                {
                    if (File.Exists(intermediatePath))
                        File.Delete(intermediatePath);
                }
                catch (Exception) { }
                try
                {
                    if (File.Exists(outputPath))
                        File.Delete(outputPath);
                }
                catch (Exception) { }
            }
        }

        private static async Task<Format> GetFormat(string path, CancellationToken token)
        {
            var arguments = $"-v quiet -of json -hide_banner -show_entries format \"{path}\"";
            var proccessInfo = new ProcessStartInfo
            {
                FileName = "ffprobe",
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
            };
            using var ffprobe = Process.Start(proccessInfo);

            var resultString = await ffprobe.StandardOutput.ReadToEndAsync();
            var result = JsonSerializer.Deserialize<FfprobeRoot>(resultString);
            return result.Format;
        }
    }
}
