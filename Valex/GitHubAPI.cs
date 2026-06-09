using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Octokit;
using Valex.Assets.Classes;

public class GitHubAPI
{
	private readonly GitHubClient _client;

	public GitHubAPI(string accessToken)
	{
		try
		{
			_client = new GitHubClient(new ProductHeaderValue("Valex"))
			{
				Credentials = new Credentials(accessToken)
			};
			Globals.githubValidated = true;
		}
		catch
		{
			Globals.githubValidated = false;
		}
	}

	public async Task<string> UploadFile(string filePath, string targetPath, string branch = "main", string commitMessage = null)
	{
		try
		{
			string fileContent = File.ReadAllText(filePath);
			string sha = null;
			bool fileExists = false;
			try
			{
				IReadOnlyList<RepositoryContent> existingFile = await _client.Repository.Content.GetAllContentsByRef(Globals.githubUsername, "Valex", targetPath, branch);
				if (existingFile.Count > 0)
				{
					sha = existingFile[0].Sha;
					fileExists = true;
				}
			}
			catch (NotFoundException)
			{
				fileExists = false;
			}
			commitMessage = commitMessage ?? ("Upload " + Path.GetFileName(filePath));
			if (fileExists)
			{
				UpdateFileRequest updateRequest = new UpdateFileRequest(commitMessage, fileContent, sha, branch);
				return (await _client.Repository.Content.UpdateFile(Globals.githubUsername, "Valex", targetPath, updateRequest)).Content.HtmlUrl;
			}
			CreateFileRequest createRequest = new CreateFileRequest(commitMessage, fileContent, branch);
			return (await _client.Repository.Content.CreateFile(Globals.githubUsername, "Valex", targetPath, createRequest)).Content.HtmlUrl;
		}
		catch (Exception ex2)
		{
			Exception ex3 = ex2;
			MessageBox.Show(ex3.Message);
			return ex3.Message;
		}
	}

	public async Task<string> CreateRepository(string description = "", bool isPrivate = false, bool autoInit = true)
	{
		try
		{
			NewRepository newRepo = new NewRepository("Valex")
			{
				Description = description,
				Private = isPrivate,
				AutoInit = autoInit
			};
			return (await _client.Repository.Create(newRepo)).CloneUrl;
		}
		catch (Exception ex)
		{
			Exception ex2 = ex;
			MessageBox.Show(ex2.Message);
			return ex2.Message;
		}
	}

	public async Task<string> GetFileContent(string fileName, string branch = "main")
	{
		try
		{
			IReadOnlyList<RepositoryContent> fileContent = await _client.Repository.Content.GetAllContentsByRef(Globals.githubUsername, "Valex", fileName, branch);
			if (fileContent.Count > 0)
			{
				return fileContent[0].Content;
			}
			return "File not found";
		}
		catch (Exception ex)
		{
			Exception ex2 = ex;
			return "Error loading file: " + ex2.Message;
		}
	}

	public async Task<List<string>> GetRepositoryFileNames(string branch = "main")
	{
		try
		{
			List<string> fileNames = new List<string>();
			foreach (RepositoryContent item in await _client.Repository.Content.GetAllContentsByRef(Globals.githubUsername, "Valex", "/", branch))
			{
				if (item.Type == ContentType.File)
				{
					fileNames.Add(item.Name);
				}
			}
			return fileNames;
		}
		catch (Exception ex)
		{
			Exception ex2 = ex;
			MessageBox.Show("Error getting file names: " + ex2.Message);
			return new List<string>();
		}
	}
}
