
using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/auctions")]
public class AuctionsController : ControllerBase
{
    private readonly AuctionDbContext _context;
    private readonly IMapper _mapper;

    public AuctionsController(AuctionDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions()
    {
        var auctions = await _context.Auctions
                    .Include(x => x.Item)
                    .OrderBy(x => x.Item.Make)
                    .ToListAsync();
        return _mapper.Map<List<AuctionDto>>(auctions);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
    {
        var auction = await _context.Auctions
            .Include(x => x.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (auction == null)
        {
            return NotFound();
        }

        return _mapper.Map<AuctionDto>(auction);
    }

    [HttpPost]
    public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto createAuctionDto)
    {
        try
        {
            var auction = _mapper.Map<Auction>(createAuctionDto);

            // Set additional properties
            auction.Id = Guid.NewGuid();
            auction.CreatedAt = DateTime.UtcNow;
            auction.Seller = "Hemant"; // Assuming you're using authentication
            auction.Status = Status.Live;

            // Add the auction to the context
            _context.Auctions.Add(auction);

            // Save changes to the database
            var result = await _context.SaveChangesAsync() > 0;

            if (!result) return BadRequest("Failed to create auction");

            // Return the created auction
            return CreatedAtAction(nameof(GetAuctionById),
                new { id = auction.Id },
                _mapper.Map<AuctionDto>(auction));
        }
        catch (Exception ex)
        {
            // Log the exception
            return StatusCode(500, "An error occurred while creating the auction");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAuction(Guid id, UpdateAuctionDto updateAuctionDto)
    {
        var auction = await _context.Auctions
            .Include(x => x.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (auction == null) return NotFound();

        // Check if the current user is the seller of the auction
        //if (auction.Seller != User.Identity.Name) return Forbid();

        // Update the auction properties
        if (updateAuctionDto.Make != null) auction.Item.Make = updateAuctionDto.Make;
        if (updateAuctionDto.Model != null) auction.Item.Model = updateAuctionDto.Model;
        if (updateAuctionDto.Color != null) auction.Item.Color = updateAuctionDto.Color;
        auction.Item.Mileage = updateAuctionDto.Mileage;
        auction.Item.Year = updateAuctionDto.Year;

        auction.UpdatedAt = DateTime.UtcNow;

        try
        {
            var result = await _context.SaveChangesAsync() > 0;

            if (result) return Ok(_mapper.Map<AuctionDto>(auction));

            return BadRequest("Problem updating auction");
        }
        catch (Exception ex)
        {
            // Log the exception
            return StatusCode(500, "An error occurred while updating the auction");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAuction(Guid id)
    {
        var auction = await _context.Auctions.FindAsync(id);

        if (auction == null) return NotFound();

        // Check if the current user is the seller of the auction
        //if (auction.Seller != User.Identity.Name) return Forbid();

        // Check if the auction is already finished
        if (auction.Status != Status.Live) return BadRequest("Cannot delete finished auction");

        _context.Auctions.Remove(auction);

        try
        {
            var result = await _context.SaveChangesAsync() > 0;

            if (result) return Ok();

            return BadRequest("Problem deleting auction");
        }
        catch (Exception ex)
        {
            // Log the exception
            return StatusCode(500, "An error occurred while deleting the auction");
        }
    }
}