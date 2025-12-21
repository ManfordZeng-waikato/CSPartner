using Application.DTOs.Comment;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Mappings;
using Domain.Comments;
using Domain.Exceptions;


namespace Application.Services;

public class CommentService : ICommentService
{
    private readonly ICommentRepository _commentRepository;
    private readonly IVideoRepository _videoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CommentService(
        ICommentRepository commentRepository,
        IVideoRepository videoRepository,
        IUnitOfWork unitOfWork)
    {
        _commentRepository = commentRepository;
        _videoRepository = videoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<CommentDto>> GetVideoCommentsAsync(Guid videoId)
    {
        var comments = await _commentRepository.GetCommentsByVideoIdAsync(videoId);
        return comments.Select(c => c.ToDto());
    }

    public async Task<CommentDto> GetCommentByIdAsync(Guid commentId)
    {
        var comment = await _commentRepository.GetCommentByIdAsync(commentId) ?? throw new CommentNotFoundException(commentId);
        return comment.ToDto();
    }

    public async Task<CommentDto> CreateCommentAsync(Guid videoId, Guid userId, string content, Guid? parentCommentId = null)
    {
        var video = await _videoRepository.GetVideoByIdAsync(videoId);
        if (video == null)
            throw new VideoNotFoundException(videoId);

        if (parentCommentId.HasValue)
        {
            var parentComment = await _commentRepository.GetCommentByIdAsync(parentCommentId.Value);
            if (parentComment == null)
                throw new CommentNotFoundException(parentCommentId.Value);
        }

        var comment = new Comment(videoId, userId, content, parentCommentId);
        await _commentRepository.AddAsync(comment);
        video.ApplyCommentAdded();
        await _videoRepository.UpdateAsync(video);
        await _unitOfWork.SaveChangesAsync();
        return comment.ToDto();
    }

    public async Task<bool> UpdateCommentAsync(Guid commentId, Guid userId, string content)
    {
        var comment = await _commentRepository.GetCommentByIdAsync(commentId);
        if (comment == null || comment.UserId != userId)
            return false;

        if (comment.IsDeleted)
            return false;

        comment.SetContent(content);
        await _commentRepository.UpdateAsync(comment);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteCommentAsync(Guid commentId, Guid userId)
    {
        var comment = await _commentRepository.GetCommentByIdAsync(commentId);
        if (comment == null || comment.UserId != userId)
            return false;

        var video = await _videoRepository.GetVideoByIdAsync(comment.VideoId);
        if (video != null)
        {
            video.ApplyCommentRemoved();
            await _videoRepository.UpdateAsync(video);
        }

        comment.SoftDelete();
        await _commentRepository.UpdateAsync(comment);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}

