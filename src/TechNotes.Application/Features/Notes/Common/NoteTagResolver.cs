using System.ComponentModel.DataAnnotations;
using TechNotes.Application.Interfaces.Repositories;
using TechNotes.Domain.Entities.Notes;

namespace TechNotes.Application.Features.Notes.Common;

public static class NoteTagResolver
{
    private const int MaxTags = 20;
    private const int MaxTagLength = 50;

    public static async Task<List<NoteTag>> ResolveAsync(
        IEnumerable<string>? tagNames,
        ITagRepository tagRepository,
        CancellationToken cancellationToken)
    {
        var names = (tagNames ?? [])
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (names.Count > MaxTags)
        {
            throw new ValidationException($"A note can have at most {MaxTags} tags.");
        }

        if (names.Count == 0)
        {
            return [];
        }

        if (names.Any(name => name.Length > MaxTagLength))
        {
            throw new ValidationException($"Each tag must be {MaxTagLength} characters or fewer.");
        }

        var existing = await tagRepository.GetByNamesAsync(names, cancellationToken);
        var byName = existing.ToDictionary(tag => tag.Name, StringComparer.OrdinalIgnoreCase);
        var noteTags = new List<NoteTag>(names.Count);

        foreach (var name in names)
        {
            if (!byName.TryGetValue(name, out var tag))
            {
                tag = new Tag { Name = name };
                await tagRepository.AddAsync(tag, cancellationToken);
                byName[name] = tag;
            }

            noteTags.Add(new NoteTag { Tag = tag });
        }

        return noteTags;
    }
}
