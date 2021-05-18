namespace VaporStore.DataProcessor
{
	using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using Castle.Core.Internal;
    using Data;
    using Microsoft.EntityFrameworkCore.Internal;
    using Newtonsoft.Json;
    using VaporStore.Data.Models;
    using VaporStore.Data.Models.Enums;
    using VaporStore.DataProcessor.Dto.Import;

    public static class Deserializer
	{
		public const string ErrorMessage = "Invalid Data";
		public const string SuccsessfulImportGameMessage = "Added {0} ({1}) with {2} tags";
		public const string SuccsessfulImportUserMessage = "Imported {0} with {1} cards";
		public const string SuccsessfulImportPurchaseMessage = "Imported {0} for {1}";
		public static string ImportGames(VaporStoreDbContext context, string jsonString)
		{
			ImportGameDto[] gameDtos = JsonConvert.DeserializeObject<ImportGameDto[]>(jsonString);
			StringBuilder result = new StringBuilder();
			List<Game> gamesToAdd = new List<Game>();
            foreach (var gameDto in gameDtos)
            {

                if (!IsValid(gameDto))
                {
					result.AppendLine(ErrorMessage);
					throw new Exception(ErrorMessage);
					continue;
                }
                if (gameDto.Tags.IsNullOrEmpty())
                {
					result.AppendLine(ErrorMessage);
					continue;
                }
				DateTime validDate;

				bool isDateValid = DateTime.TryParseExact(gameDto.ReleaseDate, "yyyy-MM-dd", CultureInfo.InvariantCulture,
					DateTimeStyles.None , out validDate);
                if (!isDateValid)
                {
					result.AppendLine(ErrorMessage);
					continue;
				}

				Game game = new Game
				{
					
					
					
				};
				if (!gamesToAdd.Any(g=>g.Developer.Name==gameDto.Developer))
                {
					Developer developer = new Developer
					{
						Name = gameDto.Developer
					};
					game.Developer = developer;
                }
                else
                {
					Game gameWithDeveloper = gamesToAdd.FirstOrDefault(g => g.Developer.Name == gameDto.Developer);
					game.Developer = gameWithDeveloper.Developer;
                }
				if (!gamesToAdd.Any(g => g.Genre.Name == gameDto.Genre))
				{
					Genre genre = new Genre
					{
						Name = gameDto.Genre
					};
					game.Genre = genre;
					
				}
                else
                {
					Game gameWithGenre = gamesToAdd.FirstOrDefault(g => g.Genre.Name ==gameDto.Genre);
					game.Genre =gameWithGenre.Genre;
                }

				
				foreach (var tag in gameDto.Tags)
                {
                    if (!gamesToAdd.Any(g=>g.GameTags.Any(gt=>gt.Tag.Name==tag)))
                    {
						Tag newTag = new Tag
						{
							Name=tag
						};
						GameTag gameTag = new GameTag
						{
							Tag = newTag,
						};
						game.GameTags.Add(gameTag);
                    }
                    else
                    {
						Game gameWithTag = gamesToAdd.FirstOrDefault(g => g.GameTags.Any(gt => gt.Tag.Name == tag));
						GameTag gameTag =gameWithTag.GameTags.FirstOrDefault(gt=>gt.Tag.Name==tag);
						game.GameTags.Add(gameTag);
                    }
                }
				gamesToAdd.Add(game);
				result.AppendLine(
					String.Format(
						SuccsessfulImportGameMessage,
						game.Name,
						game.Genre.Name,
						game.GameTags.Count));
            }
			context.AddRange(gamesToAdd);
			context.SaveChanges();
			return result.ToString().TrimEnd();
		}

		public static string ImportUsers(VaporStoreDbContext context, string jsonString)
		{
			ImportUserDto[] usersDto = JsonConvert.DeserializeObject<ImportUserDto[]>(jsonString);
			StringBuilder result = new StringBuilder();
			List<User> usersToAdd = new List<User>();
            foreach (var userDto in usersDto)
            {
                if (!IsValid(userDto))
                {
                    result.AppendLine(ErrorMessage);
                    continue;
                }
				bool notValidCard = false;
                foreach (var cardDto in userDto.Cards)
                {
                    if (!IsValid(cardDto))
                    {
						notValidCard = true;
						break;
                    }
                }
				if (notValidCard==true)
                {
					result.AppendLine(ErrorMessage);
					continue;
				}
				User user = new User
				{
					Username=userDto.UserName,
					FullName=userDto.FullName,
					Email=userDto.Email,
					Age=userDto.Age,
					
				};
                foreach (var cardDto in userDto.Cards)
                {
					Card card = new Card
					{
						Number=cardDto.Number,
						Cvc=cardDto.Cvc,
						Type=(CardType)cardDto.Type,
					};
					user.Cards.Add(card);
                }
				usersToAdd.Add(user);
				result.AppendLine(
					String.Format(SuccsessfulImportUserMessage,
					user.Username,
					user.Cards.Count));




			}
			
			context.Users.AddRange(usersToAdd);
			context.SaveChanges();
			return result.ToString().TrimEnd();
		}

		public static string ImportPurchases(VaporStoreDbContext context, string xmlString)
		{
			StringBuilder sb = new StringBuilder();
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(ImportPurchaseDto[]), new XmlRootAttribute("Purchases"));
			using (StringReader reader = new StringReader(xmlString))
			{

				ImportPurchaseDto[] purchaseDtos = (ImportPurchaseDto[])xmlSerializer.Deserialize(reader);

				List<Purchase> purchasesToAdd = new List<Purchase>();
                foreach (var purchaseDto in purchaseDtos)
                {
                    if (!IsValid(purchaseDto))
                    {
						sb.AppendLine(ErrorMessage);
						continue;
                    }
					DateTime validDate;
					bool isDateValid = DateTime.TryParseExact(purchaseDto.Date, "dd/MM/yyyy HH:mm"
						, CultureInfo.InvariantCulture, DateTimeStyles.None, out validDate);
					Purchase purchase = new Purchase
					{
						Game=context.Games.FirstOrDefault(g=>g.Name==purchaseDto.Title),
						ProductKey = purchaseDto.Key,
						Card=context.Cards.FirstOrDefault(c=>c.Number==purchaseDto.CardNumber),
						Type=(PurchaseType)purchaseDto.Type,
						Date=validDate
						
					};
					purchasesToAdd.Add(purchase);
					sb.AppendLine(String.Format(SuccsessfulImportPurchaseMessage, purchase.Game.Name, purchase.Card.User.Username));

                }
				context.Purchases.AddRange(purchasesToAdd);
				context.SaveChanges();
				return sb.ToString().TrimEnd();
			}
		}

		private static bool IsValid(object dto)
		{
			var validationContext = new ValidationContext(dto);
			var validationResult = new List<ValidationResult>();

			return Validator.TryValidateObject(dto, validationContext, validationResult, true);
		}
	}
}