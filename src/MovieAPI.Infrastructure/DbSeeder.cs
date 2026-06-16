using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MovieAPI.Domain.Entities;
using MovieAPI.Domain.Models;

namespace MovieAPI.Infrastructure;

public static class DbSeeder
{
  public static async Task SeedAsync(IServiceProvider services)
  {
    using var scope = services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

    if (await db.Movies.AnyAsync()) return;

    logger.LogInformation("Seeding development database...");

    var now = DateTime.UtcNow;

    // --- Genres ---
    Genre G(string name, string slug) =>
        new() { Id = Guid.NewGuid(), Name = name, Slug = slug, CreatedAt = now, UpdatedAt = now };

    var action = G("Action", "action");
    var drama = G("Drama", "drama");
    var thriller = G("Thriller", "thriller");
    var comedy = G("Comedy", "comedy");
    var scifi = G("Sci-Fi", "sci-fi");
    var horror = G("Horror", "horror");
    var romance = G("Romance", "romance");
    var adventure = G("Adventure", "adventure");
    var crime = G("Crime", "crime");
    var animation = G("Animation", "animation");

    db.Genres.AddRange(action, drama, thriller, comedy, scifi, horror, romance, adventure, crime, animation);

    // --- People ---
    Person P(string first, string last, int year, int month, int day) =>
        new() { Id = Guid.NewGuid(), FirstName = first, LastName = last, DateOfBirth = new DateOnly(year, month, day), CreatedAt = now, UpdatedAt = now };

    // Directors
    var nolan = P("Christopher", "Nolan", 1970, 7, 30);
    var spielberg = P("Steven", "Spielberg", 1946, 12, 18);
    var villeneuve = P("Denis", "Villeneuve", 1967, 10, 3);
    var scorsese = P("Martin", "Scorsese", 1942, 11, 17);
    var ridleyScott = P("Ridley", "Scott", 1937, 11, 30);
    var fincher = P("David", "Fincher", 1962, 8, 28);
    var coppola = P("Francis", "Coppola", 1939, 4, 7);
    var cameron = P("James", "Cameron", 1954, 8, 16);
    var peterJackson = P("Peter", "Jackson", 1961, 10, 31);
    var zemeckis = P("Robert", "Zemeckis", 1951, 5, 14);
    var demme = P("Jonathan", "Demme", 1944, 2, 22);
    var wachowski = P("Lilly", "Wachowski", 1967, 12, 29);
    var kubrick = P("Stanley", "Kubrick", 1928, 7, 26);

    // Writers
    var jonathanNolan = P("Jonathan", "Nolan", 1976, 6, 6);
    var marioPuzo = P("Mario", "Puzo", 1920, 10, 15);
    var hamptonFancher = P("Hampton", "Fancher", 1938, 7, 6);
    var ericRoth = P("Eric", "Roth", 1945, 3, 22);

    // Actors
    var diCaprio = P("Leonardo", "DiCaprio", 1974, 11, 11);
    var tomHanks = P("Tom", "Hanks", 1956, 7, 9);
    var streep = P("Meryl", "Streep", 1949, 6, 22);
    var blanchett = P("Cate", "Blanchett", 1969, 5, 14);
    var freeman = P("Morgan", "Freeman", 1937, 6, 1);
    var bradPitt = P("Brad", "Pitt", 1963, 12, 18);
    var jodieFoster = P("Jodie", "Foster", 1962, 11, 19);
    var anthonyHopkins = P("Anthony", "Hopkins", 1937, 12, 31);
    var weaver = P("Sigourney", "Weaver", 1949, 10, 8);
    var alPacino = P("Al", "Pacino", 1940, 4, 25);
    var deNiro = P("Robert", "De Niro", 1943, 8, 17);
    var heathLedger = P("Heath", "Ledger", 1979, 4, 4);
    var nataliePortman = P("Natalie", "Portman", 1981, 6, 9);
    var christianBale = P("Christian", "Bale", 1974, 1, 30);
    var mattDamon = P("Matt", "Damon", 1970, 10, 8);
    var emmaStone = P("Emma", "Stone", 1988, 11, 6);
    var ryanGosling = P("Ryan", "Gosling", 1980, 11, 12);
    var chalamet = P("Timothee", "Chalamet", 1995, 12, 27);
    var zendaya = P("Zendaya", "Coleman", 1996, 9, 1);
    var keanuReeves = P("Keanu", "Reeves", 1964, 9, 2);
    var harrisonFord = P("Harrison", "Ford", 1942, 7, 13);
    var fishburne = P("Laurence", "Fishburne", 1961, 7, 30);
    var carrieAnneMoss = P("Carrie-Anne", "Moss", 1967, 8, 21);
    var jackNicholson = P("Jack", "Nicholson", 1937, 4, 22);

    db.Persons.AddRange(
        nolan, spielberg, villeneuve, scorsese, ridleyScott, fincher, coppola,
        cameron, peterJackson, zemeckis, demme, wachowski, kubrick,
        jonathanNolan, marioPuzo, hamptonFancher, ericRoth,
        diCaprio, tomHanks, streep, blanchett, freeman, bradPitt, jodieFoster,
        anthonyHopkins, weaver, alPacino, deNiro, heathLedger, nataliePortman,
        christianBale, mattDamon, emmaStone, ryanGosling, chalamet, zendaya,
        keanuReeves, harrisonFord, fishburne, carrieAnneMoss, jackNicholson
    );

    // --- Helpers ---
    Movie M(string title, int y, int mo, int d, string plot, int runtime) =>
        new() { Id = Guid.NewGuid(), Title = title, ReleaseDate = new DateOnly(y, mo, d), PlotSummery = plot, RuntimeMinutes = runtime, CreatedAt = now, UpdatedAt = now };

    MovieDetail D(Guid movieId, string synopsis, string lang, int budget) =>
        new() { Id = Guid.NewGuid(), MovieId = movieId, Synopsis = synopsis, Language = lang, Budget = budget, CreatedAt = now, UpdatedAt = now };

    CastCrew CC(Guid movieId, Guid personId, PersonRole role) =>
        new() { Id = Guid.NewGuid(), MovieId = movieId, PersonId = personId, Role = role, CreatedAt = now, UpdatedAt = now };

    MovieGenre MG(Guid movieId, Genre genre) =>
        new() { Id = Guid.NewGuid(), MovieId = movieId, GenreId = genre.Id, CreatedAt = now, UpdatedAt = now };

    Review Rev(Guid movieId, string author, string body, int score) =>
        new() { Id = Guid.NewGuid(), MovieId = movieId, AuthorName = author, Body = body, Score = score, CreatedAt = now, UpdatedAt = now };

    var movies = new List<Movie>();
    var details = new List<MovieDetail>();
    var castCrews = new List<CastCrew>();
    var movieGenres = new List<MovieGenre>();
    var reviews = new List<Review>();

    // 1 — The Dark Knight
    var darkKnight = M("The Dark Knight", 2008, 7, 18, "Batman must accept his role as Gotham's guardian when the Joker unleashes chaos across the city.", 152);
    movies.Add(darkKnight);
    details.Add(D(darkKnight.Id, "With the help of Lt. Jim Gordon and DA Harvey Dent, Batman sets out to dismantle remaining criminal organizations plaguing the streets. The partnership proves effective, but soon they find themselves prey to a rising criminal mastermind known as the Joker, who seeks to plunge Gotham into anarchy and expose Batman's true identity.", "English", 185_000_000));
    castCrews.AddRange([CC(darkKnight.Id, nolan.Id, PersonRole.Director), CC(darkKnight.Id, jonathanNolan.Id, PersonRole.Writer), CC(darkKnight.Id, christianBale.Id, PersonRole.Cast), CC(darkKnight.Id, heathLedger.Id, PersonRole.Cast), CC(darkKnight.Id, freeman.Id, PersonRole.Cast)]);
    movieGenres.AddRange([MG(darkKnight.Id, action), MG(darkKnight.Id, crime), MG(darkKnight.Id, thriller)]);
    reviews.AddRange([Rev(darkKnight.Id, "FilmFanatic99", "Heath Ledger's Joker is one of cinema's greatest villain performances. An absolute masterpiece.", 10), Rev(darkKnight.Id, "CinemaScholar", "Nolan redefined what a superhero film can be. Dark, complex, and utterly gripping.", 9)]);

    // 2 — Inception
    var inception = M("Inception", 2010, 7, 16, "A thief who steals secrets through dream-sharing technology is tasked with planting an idea into a target's mind.", 148);
    movies.Add(inception);
    details.Add(D(inception.Id, "Dom Cobb is a skilled thief who steals valuable secrets from deep within the subconscious during the dream state. His rare ability makes him a coveted player in the world of corporate espionage, but has cost him everything he loves. Cobb is offered a chance at redemption: one last job that could give him his life back if he can accomplish the impossible — inception.", "English", 160_000_000));
    castCrews.AddRange([CC(inception.Id, nolan.Id, PersonRole.Director), CC(inception.Id, diCaprio.Id, PersonRole.Cast), CC(inception.Id, mattDamon.Id, PersonRole.Cast), CC(inception.Id, emmaStone.Id, PersonRole.Cast)]);
    movieGenres.AddRange([MG(inception.Id, action), MG(inception.Id, scifi), MG(inception.Id, thriller)]);
    reviews.AddRange([Rev(inception.Id, "DreamWeaver", "Mind-bending and visually spectacular. The ending will haunt you for days.", 10), Rev(inception.Id, "PopcornCritic", "A dense but rewarding thriller that demands your full attention.", 8)]);

    // 3 — Interstellar
    var interstellar = M("Interstellar", 2014, 11, 7, "A team of explorers travels through a wormhole in space in an attempt to ensure humanity's survival.", 169);
    movies.Add(interstellar);
    details.Add(D(interstellar.Id, "Earth's future has been ravaged by crop blight and dust storms, threatening humanity's survival. Interstellar chronicles the adventures of a group of explorers who travel through a wormhole in space to find a new planet suitable for human life. Former NASA pilot Cooper leads the mission, leaving behind his family and facing the unknown of space and time.", "English", 165_000_000));
    castCrews.AddRange([CC(interstellar.Id, nolan.Id, PersonRole.Director), CC(interstellar.Id, jonathanNolan.Id, PersonRole.Writer), CC(interstellar.Id, mattDamon.Id, PersonRole.Cast), CC(interstellar.Id, nataliePortman.Id, PersonRole.Cast), CC(interstellar.Id, anthonyHopkins.Id, PersonRole.Cast)]);
    movieGenres.AddRange([MG(interstellar.Id, scifi), MG(interstellar.Id, drama), MG(interstellar.Id, adventure)]);
    reviews.AddRange([Rev(interstellar.Id, "SpaceNerd", "Emotionally devastating and scientifically ambitious. Hans Zimmer's score is breathtaking.", 10), Rev(interstellar.Id, "ReviewerJane", "The third act loses some clarity but the journey is worth every minute.", 8)]);

    // 4 — Schindler's List
    var schindlersList = M("Schindler's List", 1993, 11, 30, "A German industrialist saves thousands of Jewish lives by employing them in his factories during the Holocaust.", 195);
    movies.Add(schindlersList);
    details.Add(D(schindlersList.Id, "In German-occupied Poland during World War II, industrialist Oskar Schindler gradually becomes concerned for his Jewish workforce after witnessing their persecution by the Nazis. The film follows his transformation from opportunist to humanitarian as he spends his entire fortune to protect his workers from the death camps, ultimately saving over a thousand lives.", "English", 22_000_000));
    castCrews.AddRange([CC(schindlersList.Id, spielberg.Id, PersonRole.Director), CC(schindlersList.Id, streep.Id, PersonRole.Cast), CC(schindlersList.Id, blanchett.Id, PersonRole.Cast)]);
    movieGenres.AddRange([MG(schindlersList.Id, drama)]);
    reviews.AddRange([Rev(schindlersList.Id, "HistoryBuff", "Spielberg's most important film. A devastating and essential piece of cinema.", 10), Rev(schindlersList.Id, "ArtHouseAmy", "Masterfully shot in black and white. The scene with the red coat is unforgettable.", 10)]);

    // 5 — Jurassic Park
    var jurassicPark = M("Jurassic Park", 1993, 6, 11, "A theme park of cloned dinosaurs goes catastrophically wrong when the creatures escape and begin hunting visitors.", 127);
    movies.Add(jurassicPark);
    details.Add(D(jurassicPark.Id, "During a preview tour of a theme park populated with genetically engineered dinosaurs, a power failure causes the creatures to break free. A small group of survivors including paleontologists, a mathematician, and the park owner's grandchildren must battle to escape the island as prehistoric predators roam freely across the facility.", "English", 63_000_000));
    castCrews.AddRange([CC(jurassicPark.Id, spielberg.Id, PersonRole.Director), CC(jurassicPark.Id, harrisonFord.Id, PersonRole.Cast), CC(jurassicPark.Id, emmaStone.Id, PersonRole.Cast)]);
    movieGenres.AddRange([MG(jurassicPark.Id, adventure), MG(jurassicPark.Id, scifi)]);
    reviews.AddRange([Rev(jurassicPark.Id, "DinoFan", "The T-Rex breakout scene scared me as a kid and still holds up perfectly today.", 9), Rev(jurassicPark.Id, "BlockbusterBob", "Groundbreaking CGI that still looks impressive. A timeless adventure.", 9)]);

    // 6 — Dune
    var dune = M("Dune", 2021, 10, 22, "A noble heir travels to the most dangerous planet in the universe to secure its most precious resource.", 155);
    movies.Add(dune);
    details.Add(D(dune.Id, "Paul Atreides, a brilliant and gifted young man born into a great destiny beyond his understanding, must travel to the most dangerous planet in the universe to ensure the future of his family and his people. As malevolent forces clash over control of Arrakis and its invaluable spice, only those who conquer their fears will survive.", "English", 165_000_000));
    castCrews.AddRange([CC(dune.Id, villeneuve.Id, PersonRole.Director), CC(dune.Id, ericRoth.Id, PersonRole.Writer), CC(dune.Id, chalamet.Id, PersonRole.Cast), CC(dune.Id, zendaya.Id, PersonRole.Cast), CC(dune.Id, ryanGosling.Id, PersonRole.Cast)]);
    movieGenres.AddRange([MG(dune.Id, scifi), MG(dune.Id, adventure)]);
    reviews.AddRange([Rev(dune.Id, "ScifiLover", "Villeneuve's most ambitious work yet. Visually stunning and epically scaled.", 10), Rev(dune.Id, "BookFan", "A faithful and gorgeous adaptation. The world-building is extraordinary.", 9)]);

    // 7 — Blade Runner 2049
    var bladeRunner = M("Blade Runner 2049", 2017, 10, 6, "A new blade runner unearths a long-buried secret that threatens to plunge what's left of society into chaos.", 164);
    movies.Add(bladeRunner);
    details.Add(D(bladeRunner.Id, "Officer K, a new blade runner for the Los Angeles Police Department, unearths a long-buried secret that has the potential to plunge what's left of society into chaos. His discovery leads him on a quest to find Rick Deckard, a former blade runner who's been missing for thirty years, in a visually breathtaking neo-noir world.", "English", 150_000_000));
    castCrews.AddRange([CC(bladeRunner.Id, villeneuve.Id, PersonRole.Director), CC(bladeRunner.Id, hamptonFancher.Id, PersonRole.Writer), CC(bladeRunner.Id, ryanGosling.Id, PersonRole.Cast), CC(bladeRunner.Id, harrisonFord.Id, PersonRole.Cast), CC(bladeRunner.Id, blanchett.Id, PersonRole.Cast)]);
    movieGenres.AddRange([MG(bladeRunner.Id, scifi), MG(bladeRunner.Id, thriller)]);
    reviews.AddRange([Rev(bladeRunner.Id, "NeoNoir", "A meditative and visually breathtaking sequel that surpasses the original.", 10), Rev(bladeRunner.Id, "PacingPete", "Slow burn but worth it. Roger Deakins' cinematography is otherworldly.", 8)]);

    // 8 — Goodfellas
    var goodfellas = M("Goodfellas", 1990, 9, 19, "The rise and fall of Henry Hill, a mobster who worked his way up the ranks of the New York Mafia.", 146);
    movies.Add(goodfellas);
    details.Add(D(goodfellas.Id, "Henry Hill and his friends work their way up through the mob hierarchy as they pursue their dream of being gangsters. The film follows thirty years of Hill's life, from his childhood in Brooklyn to his days as a feared member of the Lucchese crime family and eventual downfall as a federal informant.", "English", 25_000_000));
    castCrews.AddRange([CC(goodfellas.Id, scorsese.Id, PersonRole.Director), CC(goodfellas.Id, deNiro.Id, PersonRole.Cast), CC(goodfellas.Id, bradPitt.Id, PersonRole.Cast)]);
    movieGenres.AddRange([MG(goodfellas.Id, crime), MG(goodfellas.Id, drama)]);
    reviews.AddRange([Rev(goodfellas.Id, "MobMovie", "As good as it gets. Scorsese at the absolute peak of his powers.", 10), Rev(goodfellas.Id, "CineClub", "The tracking shot into the Copacabana alone earns a perfect score.", 10)]);

    // 9 — The Wolf of Wall Street
    var wolfOfWallStreet = M("The Wolf of Wall Street", 2013, 12, 25, "The true story of a stockbroker who built a corrupt empire of excess and fraud on Wall Street.", 180);
    movies.Add(wolfOfWallStreet);
    details.Add(D(wolfOfWallStreet.Id, "Based on the true story of Jordan Belfort, from his rise to a wealthy stockbroker living the high life to his fall involving crime, corruption and the federal government. DiCaprio delivers a career-best performance as a man consumed by greed, excess and an unquenchable thirst for more, in Scorsese's wildest and most entertaining film.", "English", 100_000_000));
    castCrews.AddRange([CC(wolfOfWallStreet.Id, scorsese.Id, PersonRole.Director), CC(wolfOfWallStreet.Id, diCaprio.Id, PersonRole.Cast), CC(wolfOfWallStreet.Id, freeman.Id, PersonRole.Cast)]);
    movieGenres.AddRange([MG(wolfOfWallStreet.Id, comedy), MG(wolfOfWallStreet.Id, crime), MG(wolfOfWallStreet.Id, drama)]);
    reviews.AddRange([Rev(wolfOfWallStreet.Id, "WallStWatcher", "Three hours of pure entertainment. DiCaprio was robbed of the Oscar.", 10), Rev(wolfOfWallStreet.Id, "MoralCompass", "Glorifies excess a bit too much but it's impossible to look away.", 7)]);

    // 10 — Alien
    var alien = M("Alien", 1979, 6, 22, "The crew of a commercial spacecraft encounters a deadly alien creature on their return voyage home.", 117);
    movies.Add(alien);
    details.Add(D(alien.Id, "After a space merchant vessel perceives an unknown transmission as a distress call, its crew is awakened from their cryo-sleep and forced to investigate a barren planet. What they find there is a creature beyond imagination, one that will pick them off one by one in terrifying fashion. Weaver's Ripley became one of cinema's most iconic heroes.", "English", 11_000_000));
    castCrews.AddRange([CC(alien.Id, ridleyScott.Id, PersonRole.Director), CC(alien.Id, weaver.Id, PersonRole.Cast), CC(alien.Id, emmaStone.Id, PersonRole.Cast)]);
    movieGenres.AddRange([MG(alien.Id, horror), MG(alien.Id, scifi)]);
    reviews.AddRange([Rev(alien.Id, "HorrorHound", "In space no one can hear you scream — and this film will make you want to.", 10), Rev(alien.Id, "ScifiPurist", "Ridley Scott's atmosphere-building is second to none. A defining film of the genre.", 10)]);

    // 11 — Gladiator
    var gladiator = M("Gladiator", 2000, 5, 5, "A betrayed Roman general seeks revenge against a corrupt emperor by fighting his way through the gladiatorial arena.", 155);
    movies.Add(gladiator);
    details.Add(D(gladiator.Id, "When Roman General Maximus is betrayed by Commodus, the emperor's ambitious son who murders his father and seizes the throne, Maximus is enslaved and forced to become a gladiator. Rising through the ranks of the arena, he works toward revenge and the restoration of Roman democracy in this sweeping epic of honor and vengeance.", "English", 103_000_000));
    castCrews.AddRange([CC(gladiator.Id, ridleyScott.Id, PersonRole.Director), CC(gladiator.Id, anthonyHopkins.Id, PersonRole.Cast), CC(gladiator.Id, deNiro.Id, PersonRole.Cast)]);
    movieGenres.AddRange([MG(gladiator.Id, action), MG(gladiator.Id, drama), MG(gladiator.Id, adventure)]);
    reviews.AddRange([Rev(gladiator.Id, "AncientRome", "Are you not entertained? Absolutely yes. A rousing epic adventure.", 9), Rev(gladiator.Id, "ActionJunkie", "The arena sequences are thrilling and the story is genuinely moving.", 8)]);

    // 12 — Fight Club
    var fightClub = M("Fight Club", 1999, 10, 15, "An insomniac office worker and a soap salesman form an underground fight club that evolves into something far more sinister.", 139);
    movies.Add(fightClub);
    details.Add(D(fightClub.Id, "The unnamed narrator is an insomniac office worker growing detached from his materialistic life. He forms a fight club with soap salesman Tyler Durden and becomes embroiled in a soap scheme that evolves into something much larger. Fincher's subversive thriller peels back layers of masculinity, consumerism and identity to reveal a shocking truth.", "English", 63_000_000));
    castCrews.AddRange([CC(fightClub.Id, fincher.Id, PersonRole.Director), CC(fightClub.Id, bradPitt.Id, PersonRole.Cast), CC(fightClub.Id, mattDamon.Id, PersonRole.Cast)]);
    movieGenres.AddRange([MG(fightClub.Id, drama), MG(fightClub.Id, thriller)]);
    reviews.AddRange([Rev(fightClub.Id, "TwistFinder", "The twist still hits hard even on a rewatch. One of the 90s best films.", 10), Rev(fightClub.Id, "PsychStudent", "A brilliant deconstruction of modern masculinity. Fincher's finest work.", 9)]);

    // 13 — Se7en
    var sevenSins = M("Se7en", 1995, 9, 22, "Two detectives hunt a serial killer who uses the seven deadly sins as the motive for his elaborate murders.", 127);
    movies.Add(sevenSins);
    details.Add(D(sevenSins.Id, "Two homicide detectives are on a desperate hunt for a serial killer whose crimes are based on the seven deadly sins. The seasoned, soon-to-retire Somerset and rookie Detective Mills find themselves in a grim cat-and-mouse game with an eerily philosophical killer in a dark unnamed city drenched in perpetual rain. The ending is truly shocking.", "English", 33_000_000));
    castCrews.AddRange([CC(sevenSins.Id, fincher.Id, PersonRole.Director), CC(sevenSins.Id, freeman.Id, PersonRole.Cast), CC(sevenSins.Id, bradPitt.Id, PersonRole.Cast)]);
    movieGenres.AddRange([MG(sevenSins.Id, crime), MG(sevenSins.Id, thriller)]);
    reviews.AddRange([Rev(sevenSins.Id, "CrimeWriter", "What's in the box? One of cinema's most devastating endings, ever.", 10), Rev(sevenSins.Id, "ThrillerFan", "Freeman and Pitt have incredible chemistry. A flawless neo-noir.", 9)]);

    // 14 — The Godfather
    var godfather = M("The Godfather", 1972, 3, 24, "The aging patriarch of a crime dynasty transfers power to his youngest son, who becomes a ruthless mob boss.", 175);
    movies.Add(godfather);
    details.Add(D(godfather.Id, "The aging patriarch of an organized crime dynasty transfers control of his clandestine empire to his reluctant youngest son, Michael. What follows is a saga of family, loyalty, and betrayal that spans years, as Michael Corleone transforms from war hero to cold-blooded mob boss. Often considered the greatest film ever made.", "English", 6_000_000));
    castCrews.AddRange([CC(godfather.Id, coppola.Id, PersonRole.Director), CC(godfather.Id, marioPuzo.Id, PersonRole.Writer), CC(godfather.Id, alPacino.Id, PersonRole.Cast), CC(godfather.Id, deNiro.Id, PersonRole.Cast), CC(godfather.Id, streep.Id, PersonRole.Cast)]);
    movieGenres.AddRange([MG(godfather.Id, crime), MG(godfather.Id, drama)]);
    reviews.AddRange([Rev(godfather.Id, "FilmScholar", "The gold standard of American cinema. Every frame is a masterclass.", 10), Rev(godfather.Id, "ClassicFan", "An offer you can't refuse: perfect acting, direction, and screenplay.", 10)]);

    // 15 — Avatar
    var avatar = M("Avatar", 2009, 12, 18, "A paraplegic Marine travels to an alien moon and must choose between following orders and protecting its people.", 162);
    movies.Add(avatar);
    details.Add(D(avatar.Id, "In the 22nd century, a paraplegic Marine is dispatched to the moon Pandora on a unique mission. He becomes torn between following his orders and protecting the world he feels is his home. Through a biological link with a native Na'vi body, Jake Sully must navigate a conflict between the Resources Development Administration and the indigenous people of Pandora.", "English", 237_000_000));
    castCrews.AddRange([CC(avatar.Id, cameron.Id, PersonRole.Director), CC(avatar.Id, weaver.Id, PersonRole.Cast), CC(avatar.Id, chalamet.Id, PersonRole.Cast)]);
    movieGenres.AddRange([MG(avatar.Id, scifi), MG(avatar.Id, adventure)]);
    reviews.AddRange([Rev(avatar.Id, "VisualFX", "The most immersive cinematic experience of its time. Pandora is breathtaking.", 9), Rev(avatar.Id, "StoryFirst", "Spectacular visuals but the script is a bit thin. Style over substance.", 7)]);

    // 16 — Titanic
    var titanic = M("Titanic", 1997, 12, 19, "A young couple from different social classes fall in love aboard the ill-fated RMS Titanic in April 1912.", 194);
    movies.Add(titanic);
    details.Add(D(titanic.Id, "Seventeen-year-old Rose is engaged to the wealthy but arrogant Cal Hockley when she meets Jack Dawson, a penniless artist who won his third-class ticket in a lucky hand of poker. Despite their class differences, they fall deeply in love, but their romance is cut short when the unsinkable Titanic strikes an iceberg and begins to sink.", "English", 200_000_000));
    castCrews.AddRange([CC(titanic.Id, cameron.Id, PersonRole.Director), CC(titanic.Id, diCaprio.Id, PersonRole.Cast), CC(titanic.Id, blanchett.Id, PersonRole.Cast)]);
    movieGenres.AddRange([MG(titanic.Id, drama), MG(titanic.Id, romance)]);
    reviews.AddRange([Rev(titanic.Id, "RomanceLover", "Still the most emotionally impactful epic I have ever seen at the cinema.", 10), Rev(titanic.Id, "HistoryGeek", "The recreation of the ship is extraordinary. Jack and Rose's love is timeless.", 9)]);

    // 17 — The Lord of the Rings: The Fellowship of the Ring
    var lotr = M("The Lord of the Rings: The Fellowship of the Ring", 2001, 12, 19, "A young hobbit must carry the One Ring to Mount Doom to destroy it, guided by a fellowship of companions.", 178);
    movies.Add(lotr);
    details.Add(D(lotr.Id, "A meek Hobbit from the Shire and eight companions set out on a journey to destroy the powerful One Ring and save Middle-earth from the Dark Lord Sauron. The fellowship faces impossible odds, breathtaking landscapes, and the slow corruption that the Ring brings to its bearer. Peter Jackson's adaptation is a monumental achievement in fantasy filmmaking.", "English", 93_000_000));
    castCrews.AddRange([CC(lotr.Id, peterJackson.Id, PersonRole.Director), CC(lotr.Id, blanchett.Id, PersonRole.Cast), CC(lotr.Id, anthonyHopkins.Id, PersonRole.Cast)]);
    movieGenres.AddRange([MG(lotr.Id, adventure), MG(lotr.Id, drama)]);
    reviews.AddRange([Rev(lotr.Id, "TolkienFan", "A landmark achievement. Jackson captured the magic of Tolkien's world perfectly.", 10), Rev(lotr.Id, "FantasyFred", "The Mines of Moria sequence alone is worth the entire runtime.", 10)]);

    // 18 — The Silence of the Lambs
    var silenceOfLambs = M("The Silence of the Lambs", 1991, 2, 14, "An FBI trainee seeks help from an incarcerated cannibal to catch a serial killer targeting young women.", 118);
    movies.Add(silenceOfLambs);
    details.Add(D(silenceOfLambs.Id, "Young FBI trainee Clarice Starling is sent to interview imprisoned psychiatrist and cannibal Dr. Hannibal Lecter to gain insights into the mind of a serial killer known as Buffalo Bill. The deeply uncomfortable exchanges between Starling and Lecter form the terrifying heart of this Oscar-winning psychological thriller.", "English", 19_000_000));
    castCrews.AddRange([CC(silenceOfLambs.Id, demme.Id, PersonRole.Director), CC(silenceOfLambs.Id, jodieFoster.Id, PersonRole.Cast), CC(silenceOfLambs.Id, anthonyHopkins.Id, PersonRole.Cast)]);
    movieGenres.AddRange([MG(silenceOfLambs.Id, crime), MG(silenceOfLambs.Id, horror), MG(silenceOfLambs.Id, thriller)]);
    reviews.AddRange([Rev(silenceOfLambs.Id, "PsychThriller", "Hopkins only has 16 minutes of screen time and still dominates every scene he's in.", 10), Rev(silenceOfLambs.Id, "FosterFan", "Foster's Clarice is one of cinema's all-time great protagonists.", 10)]);

    // 19 — The Matrix
    var matrix = M("The Matrix", 1999, 3, 31, "A computer programmer discovers that reality is a simulation controlled by machines and joins a rebellion.", 136);
    movies.Add(matrix);
    details.Add(D(matrix.Id, "Thomas Anderson, a computer programmer by day and hacker by night, is contacted by mysterious rebels who reveal that the world he knows is a simulated reality called the Matrix, created by machines to distract humans while using their bodies as an energy source. He must choose to take a red or blue pill and discover the truth.", "English", 63_000_000));
    castCrews.AddRange([CC(matrix.Id, wachowski.Id, PersonRole.Director), CC(matrix.Id, keanuReeves.Id, PersonRole.Cast), CC(matrix.Id, fishburne.Id, PersonRole.Cast), CC(matrix.Id, carrieAnneMoss.Id, PersonRole.Cast)]);
    movieGenres.AddRange([MG(matrix.Id, action), MG(matrix.Id, scifi)]);
    reviews.AddRange([Rev(matrix.Id, "RedPill", "Revolutionary in every sense of the word. Changed action and sci-fi forever.", 10), Rev(matrix.Id, "BulletTime", "Bullet-time, philosophy, kung-fu and Keanu Reeves. What more could you want?", 9)]);

    // 20 — Forrest Gump
    var forrestGump = M("Forrest Gump", 1994, 7, 6, "A kind-hearted man from Alabama unwittingly influences several major historical events across several decades.", 142);
    movies.Add(forrestGump);
    details.Add(D(forrestGump.Id, "The presidencies of Kennedy and Johnson, the events of Vietnam, Watergate and other history unfold through the perspective of an Alabama man with an IQ of 75. Though simple-minded, Forrest Gump's unwavering love for his childhood sweetheart Jenny, his mother's wisdom, and his accidental involvement in defining moments of history make him a truly extraordinary everyman.", "English", 55_000_000));
    castCrews.AddRange([CC(forrestGump.Id, zemeckis.Id, PersonRole.Director), CC(forrestGump.Id, ericRoth.Id, PersonRole.Writer), CC(forrestGump.Id, tomHanks.Id, PersonRole.Cast), CC(forrestGump.Id, nataliePortman.Id, PersonRole.Cast)]);
    movieGenres.AddRange([MG(forrestGump.Id, drama), MG(forrestGump.Id, romance)]);
    reviews.AddRange([Rev(forrestGump.Id, "HeartWarmer", "Life is like a box of chocolates — this film is the best one in the box.", 10), Rev(forrestGump.Id, "TomHanksFan", "Hanks gives the performance of a lifetime. A genuinely moving American epic.", 10)]);

    db.Movies.AddRange(movies);
    db.MovieDetails.AddRange(details);
    db.CastCrews.AddRange(castCrews);
    db.MovieGenres.AddRange(movieGenres);
    db.Reviews.AddRange(reviews);

    await db.SaveChangesAsync();

    logger.LogInformation(
        "Seeded {MovieCount} movies, {PersonCount} people, {GenreCount} genres.",
        movies.Count, db.Persons.Local.Count, db.Genres.Local.Count);
  }
}
