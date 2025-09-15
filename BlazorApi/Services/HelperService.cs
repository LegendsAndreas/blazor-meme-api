using BlazorApi.Models;

namespace BlazorApi.Services;

public class HelperService
{
    public Dictionary<string, int> SetContributedUsers(List<Tag> tags)
    {
        Dictionary<string,int> contributedUsers = new Dictionary<string, int>();
        foreach (Tag tag in tags)
        {
            if (!contributedUsers.TryAdd(string.IsNullOrEmpty(tag.AddedBy) ? "N/A" : tag.AddedBy, 1))
            {
                contributedUsers[tag.AddedBy]++;
            }
        }
            
        return contributedUsers;
    }
    
    public Dictionary<string, int> SetContributedUsers(List<Meme> memes)
    {
        Dictionary<string,int> contributedUsers = new Dictionary<string, int>();
        foreach (Meme meme in memes)
        {
            _ = string.IsNullOrEmpty(meme.AddedBy) ? meme.AddedBy = "N/A" : meme.AddedBy = meme.AddedBy;
            if (contributedUsers.ContainsKey(meme.AddedBy))
            {
                contributedUsers[meme.AddedBy]++;
            }
            else
            {
                contributedUsers.Add(meme.AddedBy, 1);
            }
            
            /*Console.WriteLine("Meme added by: "+meme.AddedBy);
            if (!contributedUsers.TryAdd(string.IsNullOrEmpty(meme.AddedBy) ? "N/A" : meme.AddedBy, 1))
            {
                Console.WriteLine("Meme added by 2: "+meme.AddedBy);
                contributedUsers[meme.AddedBy]++;
            }*/
        }
        return contributedUsers;
    }
}