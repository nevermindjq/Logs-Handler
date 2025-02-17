using Bot.Core.Models.Abstractions;

namespace Bot.Core.Models.Files.Abstractions {
	public interface IFilesRepository<TFile, in TKey, in TCreateArgs> : IReadOnlyRepository<TFile, TKey> where TFile : class, IFile<TKey> where TCreateArgs : class, IFileArgs {
		Task<TFile?> GetAsync(IFileArgs args);
		Task<TFile> CreateAsync(Stream stream, TCreateArgs args, bool dispose_stream = true);
	}
}