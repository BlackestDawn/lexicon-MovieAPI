using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MovieAPI.Domain.Constants;
using MovieAPI.Domain.Entities;
using MovieAPI.Domain.Models;

namespace MovieAPI.Infrastructure;

public static class DbSeeder
{
  public static async Task SeedAsync(IServiceProvider services)
  {
    using var scope = services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

    if (await db.Movies.AnyAsync()) return;

    logger.LogInformation("Seeding development database...");

    var now = DateTime.UtcNow;

    // --- Example users (one per role, so RBAC has something real to click
    // through locally) - reviews below are tied to these where noted, and to
    // no account at all otherwise, exercising Review.UserId's nullable path.
    async Task<ApplicationUser> ExampleUser(string email, string displayName, string role)
    {
      var existing = await userManager.FindByEmailAsync(email);
      if (existing != null) return existing;

      var user = new ApplicationUser { UserName = email, Email = email, DisplayName = displayName };
      var result = await userManager.CreateAsync(user, "Password123!");
      if (!result.Succeeded)
      {
        throw new InvalidOperationException(
            $"Failed to seed example user '{email}': {string.Join("; ", result.Errors.Select(e => e.Description))}");
      }

      await userManager.AddToRoleAsync(user, role);
      return user;
    }

    var alice = await ExampleUser("alice.n@example.com", "Alice Nguyen", Roles.User);
    var ben = await ExampleUser("ben.c@example.com", "Ben Carter", Roles.PowerUser);
    var priya = await ExampleUser("priya.s@example.com", "Priya Sharma", Roles.Moderator);
    var marcus = await ExampleUser("marcus.w@example.com", "Marcus Webb", Roles.Administrator);

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

    // --- Persons ---
    // Middle names are only set where they're a matter of public record and
    // reasonably well known - many public figures simply don't have one on file.
    Person P(string first, string? middle, string last, int year, int month, int day) =>
      new() { Id = Guid.NewGuid(), GivenName = first, MiddleName = middle, LastName = last, DateOfBirth = new DateOnly(year, month, day), CreatedAt = now, UpdatedAt = now };

    // Directors
    var nolan = P("Christopher", null, "Nolan", 1970, 7, 30);
    var spielberg = P("Steven", "Allan", "Spielberg", 1946, 12, 18);
    var villeneuve = P("Denis", null, "Villeneuve", 1967, 10, 3);
    var scorsese = P("Martin", null, "Scorsese", 1942, 11, 17);
    var ridleyScott = P("Ridley", null, "Scott", 1937, 11, 30);
    var fincher = P("David", null, "Fincher", 1962, 8, 28);
    var coppola = P("Francis", "Ford", "Coppola", 1939, 4, 7);
    var cameron = P("James", "Francis", "Cameron", 1954, 8, 16);
    var peterJackson = P("Peter", null, "Jackson", 1961, 10, 31);
    var zemeckis = P("Robert", null, "Zemeckis", 1951, 5, 14);
    var demme = P("Jonathan", null, "Demme", 1944, 2, 22);
    var wachowski = P("Lilly", null, "Wachowski", 1967, 12, 29);
    var kubrick = P("Stanley", null, "Kubrick", 1928, 7, 26);
    var darabont = P("Frank", null, "Darabont", 1959, 1, 28);
    var tarantino = P("Quentin", null, "Tarantino", 1963, 3, 27);
    var chazelle = P("Damien", null, "Chazelle", 1985, 1, 19);
    var bongJoonHo = P("Joon-ho", null, "Bong", 1969, 9, 14);
    var jordanPeele = P("Jordan", "Haworth", "Peele", 1979, 2, 21);
    var georgeMiller = P("George", null, "Miller", 1945, 3, 3);
    var joelCoen = P("Joel", null, "Coen", 1954, 11, 29);
    var ethanCoen = P("Ethan", null, "Coen", 1957, 9, 21);
    var ptAnderson = P("Paul", "Thomas", "Anderson", 1970, 6, 26);
    var wesAnderson = P("Wesley", "Wales", "Anderson", 1969, 5, 1);
    var curtiz = P("Michael", null, "Curtiz", 1886, 12, 24);
    var georgeLucas = P("George", "Walton", "Lucas", 1944, 5, 14);
    var lasseter = P("John", null, "Lasseter", 1957, 1, 12);
    var miyazaki = P("Hayao", null, "Miyazaki", 1941, 1, 5);
    var forman = P("Milos", null, "Forman", 1932, 2, 18);

    // Writers
    var jonathanNolan = P("Jonathan", null, "Nolan", 1976, 6, 6);
    var marioPuzo = P("Mario", null, "Puzo", 1920, 10, 15);
    var hamptonFancher = P("Hampton", null, "Fancher", 1938, 7, 6);
    var ericRoth = P("Eric", null, "Roth", 1945, 3, 22);

    // Actors
    var diCaprio = P("Leonardo", "Wilhelm", "DiCaprio", 1974, 11, 11);
    var tomHanks = P("Tom", "Jeffrey", "Hanks", 1956, 7, 9);
    var streep = P("Meryl", "Louise", "Streep", 1949, 6, 22);
    var blanchett = P("Cate", "Elise", "Blanchett", 1969, 5, 14);
    var freeman = P("Morgan", null, "Freeman", 1937, 6, 1);
    var bradPitt = P("Brad", null, "Pitt", 1963, 12, 18);
    var jodieFoster = P("Jodie", null, "Foster", 1962, 11, 19);
    var anthonyHopkins = P("Anthony", null, "Hopkins", 1937, 12, 31);
    var weaver = P("Sigourney", null, "Weaver", 1949, 10, 8);
    var alPacino = P("Al", "James", "Pacino", 1940, 4, 25);
    var deNiro = P("Robert", null, "De Niro", 1943, 8, 17);
    var heathLedger = P("Heath", "Andrew", "Ledger", 1979, 4, 4);
    var nataliePortman = P("Natalie", null, "Portman", 1981, 6, 9);
    var christianBale = P("Christian", "Charles", "Bale", 1974, 1, 30);
    var mattDamon = P("Matt", "Paige", "Damon", 1970, 10, 8);
    var emmaStone = P("Emma", null, "Stone", 1988, 11, 6);
    var ryanGosling = P("Ryan", "Thomas", "Gosling", 1980, 11, 12);
    var chalamet = P("Timothee", "Hal", "Chalamet", 1995, 12, 27);
    var zendaya = P("Zendaya", "Maree", "Coleman", 1996, 9, 1);
    var keanuReeves = P("Keanu", null, "Reeves", 1964, 9, 2);
    var harrisonFord = P("Harrison", null, "Ford", 1942, 7, 13);
    var fishburne = P("Laurence", "John", "Fishburne", 1961, 7, 30);
    var carrieAnneMoss = P("Carrie-Anne", null, "Moss", 1967, 8, 21);
    var jackNicholson = P("Jack", "Joseph", "Nicholson", 1937, 4, 22);
    var timRobbins = P("Tim", "Francis", "Robbins", 1958, 10, 16);
    var johnTravolta = P("John", null, "Travolta", 1954, 2, 18);
    var samuelLJackson = P("Samuel", "Leroy", "Jackson", 1948, 12, 21);
    var umaThurman = P("Uma", "Karuna", "Thurman", 1970, 4, 29);
    var milesTeller = P("Miles", null, "Teller", 1987, 2, 20);
    var jkSimmons = P("Jonathan", "Kimble", "Simmons", 1955, 1, 9);
    var songKangHo = P("Kang-ho", null, "Song", 1967, 1, 17);
    var choiWooShik = P("Woo-shik", null, "Choi", 1990, 3, 26);
    var danielKaluuya = P("Daniel", null, "Kaluuya", 1989, 2, 24);
    var allisonWilliams = P("Allison", null, "Williams", 1988, 4, 13);
    var tomHardy = P("Tom", null, "Hardy", 1977, 9, 15);
    var charlizeTheron = P("Charlize", null, "Theron", 1975, 8, 7);
    var javierBardem = P("Javier", "Angel", "Bardem", 1969, 3, 1);
    var joshBrolin = P("Josh", null, "Brolin", 1968, 2, 12);
    var tommyLeeJones = P("Tommy", "Lee", "Jones", 1946, 9, 15);
    var danielDayLewis = P("Daniel", null, "Day-Lewis", 1957, 4, 29);
    var ralphFiennes = P("Ralph", "Nathaniel", "Fiennes", 1962, 12, 22);
    var humphreyBogart = P("Humphrey", "DeForest", "Bogart", 1899, 12, 25);
    var ingridBergman = P("Ingrid", null, "Bergman", 1915, 8, 29);
    var markHamill = P("Mark", null, "Hamill", 1951, 9, 25);
    var carrieFisher = P("Carrie", "Frances", "Fisher", 1956, 10, 21);
    var michaelJFox = P("Michael", null, "Fox", 1961, 6, 9);
    var christopherLloyd = P("Christopher", null, "Lloyd", 1938, 10, 22);
    var timAllen = P("Tim", null, "Allen", 1953, 6, 13);
    var michaelClarkeDuncan = P("Michael", "Clarke", "Duncan", 1957, 12, 10);
    var jamieFoxx = P("Jamie", null, "Foxx", 1967, 12, 13);
    var christophWaltz = P("Christoph", null, "Waltz", 1956, 10, 4);
    var murrayAbraham = P("Murray", null, "Abraham", 1939, 10, 24);
    var tomHulce = P("Tom", "Edward", "Hulce", 1953, 12, 6);

    db.Persons.AddRange(
        nolan, spielberg, villeneuve, scorsese, ridleyScott, fincher, coppola,
        cameron, peterJackson, zemeckis, demme, wachowski, kubrick, darabont,
        tarantino, chazelle, bongJoonHo, jordanPeele, georgeMiller, joelCoen,
        ethanCoen, ptAnderson, wesAnderson, curtiz, georgeLucas, lasseter,
        miyazaki, forman,
        jonathanNolan, marioPuzo, hamptonFancher, ericRoth,
        diCaprio, tomHanks, streep, blanchett, freeman, bradPitt, jodieFoster,
        anthonyHopkins, weaver, alPacino, deNiro, heathLedger, nataliePortman,
        christianBale, mattDamon, emmaStone, ryanGosling, chalamet, zendaya,
        keanuReeves, harrisonFord, fishburne, carrieAnneMoss, jackNicholson,
        timRobbins, johnTravolta, samuelLJackson, umaThurman, milesTeller,
        jkSimmons, songKangHo, choiWooShik, danielKaluuya, allisonWilliams,
        tomHardy, charlizeTheron, javierBardem, joshBrolin, tommyLeeJones,
        danielDayLewis, ralphFiennes, humphreyBogart, ingridBergman,
        markHamill, carrieFisher, michaelJFox, christopherLloyd, timAllen,
        michaelClarkeDuncan, jamieFoxx, christophWaltz, murrayAbraham, tomHulce
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

    // Ties a review to one of the example accounts above, mirroring what a
    // review posted through the API (rather than seeded as freeform data) looks like.
    Review RevBy(Guid movieId, ApplicationUser user, string body, int score) =>
      new() { Id = Guid.NewGuid(), MovieId = movieId, AuthorName = user.DisplayName, UserId = user.Id, Body = body, Score = score, CreatedAt = now, UpdatedAt = now };

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
    reviews.AddRange([Rev(darkKnight.Id, "FilmFanatic99", "Heath Ledger's Joker is one of cinema's greatest villain performances. An absolute masterpiece.", 10), Rev(darkKnight.Id, "CinemaScholar", "Nolan redefined what a superhero film can be. Dark, complex, and utterly gripping.", 9), RevBy(darkKnight.Id, alice, "The best Batman film ever made, full stop. Every performance is firing on all cylinders.", 10)]);

    // 2 — Inception
    var inception = M("Inception", 2010, 7, 16, "A thief who steals secrets through dream-sharing technology is tasked with planting an idea into a target's mind.", 148);
    movies.Add(inception);
    details.Add(D(inception.Id, "Dom Cobb is a skilled thief who steals valuable secrets from deep within the subconscious during the dream state. His rare ability makes him a coveted player in the world of corporate espionage, but has cost him everything he loves. Cobb is offered a chance at redemption: one last job that could give him his life back if he can accomplish the impossible — inception.", "English", 160_000_000));
    castCrews.AddRange([CC(inception.Id, nolan.Id, PersonRole.Director), CC(inception.Id, diCaprio.Id, PersonRole.Cast), CC(inception.Id, mattDamon.Id, PersonRole.Cast), CC(inception.Id, emmaStone.Id, PersonRole.Cast)]);
    movieGenres.AddRange([MG(inception.Id, action), MG(inception.Id, scifi), MG(inception.Id, thriller)]);
    reviews.AddRange([Rev(inception.Id, "DreamWeaver", "Mind-bending and visually spectacular. The ending will haunt you for days.", 10), Rev(inception.Id, "PopcornCritic", "A dense but rewarding thriller that demands your full attention.", 8), RevBy(inception.Id, ben, "Rewatched it three times and caught something new each time. Nolan at his most ambitious.", 9)]);

    // 3 — Interstellar
    var interstellar = M("Interstellar", 2014, 11, 7, "A team of explorers travels through a wormhole in space in an attempt to ensure humanity's survival.", 169);
    movies.Add(interstellar);
    details.Add(D(interstellar.Id, "Earth's future has been ravaged by crop blight and dust storms, threatening humanity's survival. Interstellar chronicles the adventures of a group of explorers who travel through a wormhole in space to find a new planet suitable for human life. Former NASA pilot Cooper leads the mission, leaving behind his family and facing the unknown of space and time.", "English", 165_000_000));
    castCrews.AddRange([CC(interstellar.Id, nolan.Id, PersonRole.Director), CC(interstellar.Id, jonathanNolan.Id, PersonRole.Writer), CC(interstellar.Id, mattDamon.Id, PersonRole.Cast), CC(interstellar.Id, nataliePortman.Id, PersonRole.Cast), CC(interstellar.Id, anthonyHopkins.Id, PersonRole.Cast)]);
    movieGenres.AddRange([MG(interstellar.Id, scifi), MG(interstellar.Id, drama), MG(interstellar.Id, adventure)]);
    reviews.AddRange([Rev(interstellar.Id, "SpaceNerd", "Emotionally devastating and scientifically ambitious. Hans Zimmer's score is breathtaking.", 10), Rev(interstellar.Id, "ReviewerJane", "The third act loses some clarity but the journey is worth every minute.", 8), RevBy(interstellar.Id, alice, "Cried during the docking sequence. This movie gets me every single time.", 10)]);

    // 4 — Schindler's List
    var schindlersList = M("Schindler's List", 1993, 11, 30, "A German industrialist saves thousands of Jewish lives by employing them in his factories during the Holocaust.", 195);
    movies.Add(schindlersList);
    details.Add(D(schindlersList.Id, "In German-occupied Poland during World War II, industrialist Oskar Schindler gradually becomes concerned for his Jewish workforce after witnessing their persecution by the Nazis. The film follows his transformation from opportunist to humanitarian as he spends his entire fortune to protect his workers from the death camps, ultimately saving over a thousand lives.", "English", 22_000_000));
    castCrews.AddRange([CC(schindlersList.Id, spielberg.Id, PersonRole.Director), CC(schindlersList.Id, streep.Id, PersonRole.Cast), CC(schindlersList.Id, blanchett.Id, PersonRole.Cast)]);
    movieGenres.AddRange([MG(schindlersList.Id, drama)]);
    reviews.AddRange([Rev(schindlersList.Id, "HistoryBuff", "Spielberg's most important film. A devastating and essential piece of cinema.", 10), Rev(schindlersList.Id, "ArtHouseAmy", "Masterfully shot in black and white. The scene with the red coat is unforgettable.", 10), RevBy(schindlersList.Id, priya, "Impossible to watch without being profoundly moved. Essential viewing.", 10)]);

    // 5 — Jurassic Park
    var jurassicPark = M("Jurassic Park", 1993, 6, 11, "A theme park of cloned dinosaurs goes catastrophically wrong when the creatures escape and begin hunting visitors.", 127);
    movies.Add(jurassicPark);
    details.Add(D(jurassicPark.Id, "During a preview tour of a theme park populated with genetically engineered dinosaurs, a power failure causes the creatures to break free. A small group of survivors including paleontologists, a mathematician, and the park owner's grandchildren must battle to escape the island as prehistoric predators roam freely across the facility.", "English", 63_000_000));
    castCrews.AddRange([CC(jurassicPark.Id, spielberg.Id, PersonRole.Director), CC(jurassicPark.Id, harrisonFord.Id, PersonRole.Cast), CC(jurassicPark.Id, emmaStone.Id, PersonRole.Cast)]);
    movieGenres.AddRange([MG(jurassicPark.Id, adventure), MG(jurassicPark.Id, scifi)]);
    reviews.AddRange([Rev(jurassicPark.Id, "DinoFan", "The T-Rex breakout scene scared me as a kid and still holds up perfectly today.", 9), Rev(jurassicPark.Id, "BlockbusterBob", "Groundbreaking CGI that still looks impressive. A timeless adventure.", 9), Rev(jurassicPark.Id, "SpielbergStan", "The practical effects blended with early CGI still look better than most modern blockbusters.", 9)]);

    // 6 — Dune
    var dune = M("Dune", 2021, 10, 22, "A noble heir travels to the most dangerous planet in the universe to secure its most precious resource.", 155);
    movies.Add(dune);
    details.Add(D(dune.Id, "Paul Atreides, a brilliant and gifted young man born into a great destiny beyond his understanding, must travel to the most dangerous planet in the universe to ensure the future of his family and his people. As malevolent forces clash over control of Arrakis and its invaluable spice, only those who conquer their fears will survive.", "English", 165_000_000));
    castCrews.AddRange([CC(dune.Id, villeneuve.Id, PersonRole.Director), CC(dune.Id, ericRoth.Id, PersonRole.Writer), CC(dune.Id, chalamet.Id, PersonRole.Cast), CC(dune.Id, zendaya.Id, PersonRole.Cast), CC(dune.Id, ryanGosling.Id, PersonRole.Cast)]);
    movieGenres.AddRange([MG(dune.Id, scifi), MG(dune.Id, adventure)]);
    reviews.AddRange([Rev(dune.Id, "ScifiLover", "Villeneuve's most ambitious work yet. Visually stunning and epically scaled.", 10), Rev(dune.Id, "BookFan", "A faithful and gorgeous adaptation. The world-building is extraordinary.", 9), RevBy(dune.Id, ben, "Can't wait for the sequel after this. Chalamet was born to play Paul Atreides.", 9)]);

    // 7 — Blade Runner 2049
    var bladeRunner = M("Blade Runner 2049", 2017, 10, 6, "A new blade runner unearths a long-buried secret that threatens to plunge what's left of society into chaos.", 164);
    movies.Add(bladeRunner);
    details.Add(D(bladeRunner.Id, "Officer K, a new blade runner for the Los Angeles Police Department, unearths a long-buried secret that has the potential to plunge what's left of society into chaos. His discovery leads him on a quest to find Rick Deckard, a former blade runner who's been missing for thirty years, in a visually breathtaking neo-noir world.", "English", 150_000_000));
    castCrews.AddRange([CC(bladeRunner.Id, villeneuve.Id, PersonRole.Director), CC(bladeRunner.Id, hamptonFancher.Id, PersonRole.Writer), CC(bladeRunner.Id, ryanGosling.Id, PersonRole.Cast), CC(bladeRunner.Id, harrisonFord.Id, PersonRole.Cast), CC(bladeRunner.Id, blanchett.Id, PersonRole.Cast)]);
    movieGenres.AddRange([MG(bladeRunner.Id, scifi), MG(bladeRunner.Id, thriller)]);
    reviews.AddRange([Rev(bladeRunner.Id, "NeoNoir", "A meditative and visually breathtaking sequel that surpasses the original.", 10), Rev(bladeRunner.Id, "PacingPete", "Slow burn but worth it. Roger Deakins' cinematography is otherworldly.", 8), Rev(bladeRunner.Id, "SciFiSequel", "A rare sequel that expands and deepens the original instead of just repeating it.", 9)]);

    // 8 — Goodfellas
    var goodfellas = M("Goodfellas", 1990, 9, 19, "The rise and fall of Henry Hill, a mobster who worked his way up the ranks of the New York Mafia.", 146);
    movies.Add(goodfellas);
    details.Add(D(goodfellas.Id, "Henry Hill and his friends work their way up through the mob hierarchy as they pursue their dream of being gangsters. The film follows thirty years of Hill's life, from his childhood in Brooklyn to his days as a feared member of the Lucchese crime family and eventual downfall as a federal informant.", "English", 25_000_000));
    castCrews.AddRange([CC(goodfellas.Id, scorsese.Id, PersonRole.Director), CC(goodfellas.Id, deNiro.Id, PersonRole.Cast), CC(goodfellas.Id, bradPitt.Id, PersonRole.Cast)]);
    movieGenres.AddRange([MG(goodfellas.Id, crime), MG(goodfellas.Id, drama)]);
    reviews.AddRange([Rev(goodfellas.Id, "MobMovie", "As good as it gets. Scorsese at the absolute peak of his powers.", 10), Rev(goodfellas.Id, "CineClub", "The tracking shot into the Copacabana alone earns a perfect score.", 10), RevBy(goodfellas.Id, marcus, "The narration and needle drops make this required viewing for any film student.", 10)]);

    // 9 — The Wolf of Wall Street
    var wolfOfWallStreet = M("The Wolf of Wall Street", 2013, 12, 25, "The true story of a stockbroker who built a corrupt empire of excess and fraud on Wall Street.", 180);
    movies.Add(wolfOfWallStreet);
    details.Add(D(wolfOfWallStreet.Id, "Based on the true story of Jordan Belfort, from his rise to a wealthy stockbroker living the high life to his fall involving crime, corruption and the federal government. DiCaprio delivers a career-best performance as a man consumed by greed, excess and an unquenchable thirst for more, in Scorsese's wildest and most entertaining film.", "English", 100_000_000));
    castCrews.AddRange([CC(wolfOfWallStreet.Id, scorsese.Id, PersonRole.Director), CC(wolfOfWallStreet.Id, diCaprio.Id, PersonRole.Cast), CC(wolfOfWallStreet.Id, freeman.Id, PersonRole.Cast)]);
    movieGenres.AddRange([MG(wolfOfWallStreet.Id, comedy), MG(wolfOfWallStreet.Id, crime), MG(wolfOfWallStreet.Id, drama)]);
    reviews.AddRange([Rev(wolfOfWallStreet.Id, "WallStWatcher", "Three hours of pure entertainment. DiCaprio was robbed of the Oscar.", 10), Rev(wolfOfWallStreet.Id, "MoralCompass", "Glorifies excess a bit too much but it's impossible to look away.", 7), Rev(wolfOfWallStreet.Id, "ExcessCritic", "Wildly entertaining even as it indicts the very excess it depicts.", 8)]);

    // 10 — Alien
    var alien = M("Alien", 1979, 6, 22, "The crew of a commercial spacecraft encounters a deadly alien creature on their return voyage home.", 117);
    movies.Add(alien);
    details.Add(D(alien.Id, "After a space merchant vessel perceives an unknown transmission as a distress call, its crew is awakened from their cryo-sleep and forced to investigate a barren planet. What they find there is a creature beyond imagination, one that will pick them off one by one in terrifying fashion. Weaver's Ripley became one of cinema's most iconic heroes.", "English", 11_000_000));
    castCrews.AddRange([CC(alien.Id, ridleyScott.Id, PersonRole.Director), CC(alien.Id, weaver.Id, PersonRole.Cast), CC(alien.Id, emmaStone.Id, PersonRole.Cast)]);
    movieGenres.AddRange([MG(alien.Id, horror), MG(alien.Id, scifi)]);
    reviews.AddRange([Rev(alien.Id, "HorrorHound", "In space no one can hear you scream — and this film will make you want to.", 10), Rev(alien.Id, "ScifiPurist", "Ridley Scott's atmosphere-building is second to none. A defining film of the genre.", 10), Rev(alien.Id, "XenomorphXpert", "The slow-build tension in the first hour is a masterclass in dread.", 10)]);

    // 11 — Gladiator
    var gladiator = M("Gladiator", 2000, 5, 5, "A betrayed Roman general seeks revenge against a corrupt emperor by fighting his way through the gladiatorial arena.", 155);
    movies.Add(gladiator);
    details.Add(D(gladiator.Id, "When Roman General Maximus is betrayed by Commodus, the emperor's ambitious son who murders his father and seizes the throne, Maximus is enslaved and forced to become a gladiator. Rising through the ranks of the arena, he works toward revenge and the restoration of Roman democracy in this sweeping epic of honor and vengeance.", "English", 103_000_000));
    castCrews.AddRange([CC(gladiator.Id, ridleyScott.Id, PersonRole.Director), CC(gladiator.Id, anthonyHopkins.Id, PersonRole.Cast), CC(gladiator.Id, deNiro.Id, PersonRole.Cast)]);
    movieGenres.AddRange([MG(gladiator.Id, action), MG(gladiator.Id, drama), MG(gladiator.Id, adventure)]);
    reviews.AddRange([Rev(gladiator.Id, "AncientRome", "Are you not entertained? Absolutely yes. A rousing epic adventure.", 9), Rev(gladiator.Id, "ActionJunkie", "The arena sequences are thrilling and the story is genuinely moving.", 8), Rev(gladiator.Id, "ColosseumCraig", "Crowe's performance carries an already sweeping, satisfying revenge epic.", 8)]);

    // 12 — Fight Club
    var fightClub = M("Fight Club", 1999, 10, 15, "An insomniac office worker and a soap salesman form an underground fight club that evolves into something far more sinister.", 139);
    movies.Add(fightClub);
    details.Add(D(fightClub.Id, "The unnamed narrator is an insomniac office worker growing detached from his materialistic life. He forms a fight club with soap salesman Tyler Durden and becomes embroiled in a soap scheme that evolves into something much larger. Fincher's subversive thriller peels back layers of masculinity, consumerism and identity to reveal a shocking truth.", "English", 63_000_000));
    castCrews.AddRange([CC(fightClub.Id, fincher.Id, PersonRole.Director), CC(fightClub.Id, bradPitt.Id, PersonRole.Cast), CC(fightClub.Id, mattDamon.Id, PersonRole.Cast)]);
    movieGenres.AddRange([MG(fightClub.Id, drama), MG(fightClub.Id, thriller)]);
    reviews.AddRange([Rev(fightClub.Id, "TwistFinder", "The twist still hits hard even on a rewatch. One of the 90s best films.", 10), Rev(fightClub.Id, "PsychStudent", "A brilliant deconstruction of modern masculinity. Fincher's finest work.", 9), RevBy(fightClub.Id, alice, "Ahead of its time. The commentary on consumer culture only gets sharper with age.", 9)]);

    // 13 — Se7en
    var sevenSins = M("Se7en", 1995, 9, 22, "Two detectives hunt a serial killer who uses the seven deadly sins as the motive for his elaborate murders.", 127);
    movies.Add(sevenSins);
    details.Add(D(sevenSins.Id, "Two homicide detectives are on a desperate hunt for a serial killer whose crimes are based on the seven deadly sins. The seasoned, soon-to-retire Somerset and rookie Detective Mills find themselves in a grim cat-and-mouse game with an eerily philosophical killer in a dark unnamed city drenched in perpetual rain. The ending is truly shocking.", "English", 33_000_000));
    castCrews.AddRange([CC(sevenSins.Id, fincher.Id, PersonRole.Director), CC(sevenSins.Id, freeman.Id, PersonRole.Cast), CC(sevenSins.Id, bradPitt.Id, PersonRole.Cast)]);
    movieGenres.AddRange([MG(sevenSins.Id, crime), MG(sevenSins.Id, thriller)]);
    reviews.AddRange([Rev(sevenSins.Id, "CrimeWriter", "What's in the box? One of cinema's most devastating endings, ever.", 10), Rev(sevenSins.Id, "ThrillerFan", "Freeman and Pitt have incredible chemistry. A flawless neo-noir.", 9), RevBy(sevenSins.Id, ben, "That final act is one of the bleakest, most unforgettable endings in thriller history.", 9)]);

    // 14 — The Godfather
    var godfather = M("The Godfather", 1972, 3, 24, "The aging patriarch of a crime dynasty transfers power to his youngest son, who becomes a ruthless mob boss.", 175);
    movies.Add(godfather);
    details.Add(D(godfather.Id, "The aging patriarch of an organized crime dynasty transfers control of his clandestine empire to his reluctant youngest son, Michael. What follows is a saga of family, loyalty, and betrayal that spans years, as Michael Corleone transforms from war hero to cold-blooded mob boss. Often considered the greatest film ever made.", "English", 6_000_000));
    castCrews.AddRange([CC(godfather.Id, coppola.Id, PersonRole.Director), CC(godfather.Id, marioPuzo.Id, PersonRole.Writer), CC(godfather.Id, alPacino.Id, PersonRole.Cast), CC(godfather.Id, deNiro.Id, PersonRole.Cast), CC(godfather.Id, streep.Id, PersonRole.Cast)]);
    movieGenres.AddRange([MG(godfather.Id, crime), MG(godfather.Id, drama)]);
    reviews.AddRange([Rev(godfather.Id, "FilmScholar", "The gold standard of American cinema. Every frame is a masterclass.", 10), Rev(godfather.Id, "ClassicFan", "An offer you can't refuse: perfect acting, direction, and screenplay.", 10), RevBy(godfather.Id, priya, "A perfect film in every technical and dramatic sense. Nothing else comes close.", 10)]);

    // 15 — Avatar
    var avatar = M("Avatar", 2009, 12, 18, "A paraplegic Marine travels to an alien moon and must choose between following orders and protecting its people.", 162);
    movies.Add(avatar);
    details.Add(D(avatar.Id, "In the 22nd century, a paraplegic Marine is dispatched to the moon Pandora on a unique mission. He becomes torn between following his orders and protecting the world he feels is his home. Through a biological link with a native Na'vi body, Jake Sully must navigate a conflict between the Resources Development Administration and the indigenous people of Pandora.", "English", 237_000_000));
    castCrews.AddRange([CC(avatar.Id, cameron.Id, PersonRole.Director), CC(avatar.Id, weaver.Id, PersonRole.Cast), CC(avatar.Id, chalamet.Id, PersonRole.Cast)]);
    movieGenres.AddRange([MG(avatar.Id, scifi), MG(avatar.Id, adventure)]);
    reviews.AddRange([Rev(avatar.Id, "VisualFX", "The most immersive cinematic experience of its time. Pandora is breathtaking.", 9), Rev(avatar.Id, "StoryFirst", "Spectacular visuals but the script is a bit thin. Style over substance.", 7), Rev(avatar.Id, "PandoraPete", "The technical achievement alone justifies a rewatch on the biggest screen you can find.", 8)]);

    // 16 — Titanic
    var titanic = M("Titanic", 1997, 12, 19, "A young couple from different social classes fall in love aboard the ill-fated RMS Titanic in April 1912.", 194);
    movies.Add(titanic);
    details.Add(D(titanic.Id, "Seventeen-year-old Rose is engaged to the wealthy but arrogant Cal Hockley when she meets Jack Dawson, a penniless artist who won his third-class ticket in a lucky hand of poker. Despite their class differences, they fall deeply in love, but their romance is cut short when the unsinkable Titanic strikes an iceberg and begins to sink.", "English", 200_000_000));
    castCrews.AddRange([CC(titanic.Id, cameron.Id, PersonRole.Director), CC(titanic.Id, diCaprio.Id, PersonRole.Cast), CC(titanic.Id, blanchett.Id, PersonRole.Cast)]);
    movieGenres.AddRange([MG(titanic.Id, drama), MG(titanic.Id, romance)]);
    reviews.AddRange([Rev(titanic.Id, "RomanceLover", "Still the most emotionally impactful epic I have ever seen at the cinema.", 10), Rev(titanic.Id, "HistoryGeek", "The recreation of the ship is extraordinary. Jack and Rose's love is timeless.", 9), Rev(titanic.Id, "OceanicEpic", "The scale of the sinking sequence still overwhelms me every time.", 9)]);

    // 17 — The Lord of the Rings: The Fellowship of the Ring
    var lotr = M("The Lord of the Rings: The Fellowship of the Ring", 2001, 12, 19, "A young hobbit must carry the One Ring to Mount Doom to destroy it, guided by a fellowship of companions.", 178);
    movies.Add(lotr);
    details.Add(D(lotr.Id, "A meek Hobbit from the Shire and eight companions set out on a journey to destroy the powerful One Ring and save Middle-earth from the Dark Lord Sauron. The fellowship faces impossible odds, breathtaking landscapes, and the slow corruption that the Ring brings to its bearer. Peter Jackson's adaptation is a monumental achievement in fantasy filmmaking.", "English", 93_000_000));
    castCrews.AddRange([CC(lotr.Id, peterJackson.Id, PersonRole.Director), CC(lotr.Id, blanchett.Id, PersonRole.Cast), CC(lotr.Id, anthonyHopkins.Id, PersonRole.Cast)]);
    movieGenres.AddRange([MG(lotr.Id, adventure), MG(lotr.Id, drama)]);
    reviews.AddRange([Rev(lotr.Id, "TolkienFan", "A landmark achievement. Jackson captured the magic of Tolkien's world perfectly.", 10), Rev(lotr.Id, "FantasyFred", "The Mines of Moria sequence alone is worth the entire runtime.", 10), Rev(lotr.Id, "MiddleEarthMike", "Jackson's world-building set the bar for every fantasy adaptation that followed.", 10)]);

    // 18 — The Silence of the Lambs
    var silenceOfLambs = M("The Silence of the Lambs", 1991, 2, 14, "An FBI trainee seeks help from an incarcerated cannibal to catch a serial killer targeting young women.", 118);
    movies.Add(silenceOfLambs);
    details.Add(D(silenceOfLambs.Id, "Young FBI trainee Clarice Starling is sent to interview imprisoned psychiatrist and cannibal Dr. Hannibal Lecter to gain insights into the mind of a serial killer known as Buffalo Bill. The deeply uncomfortable exchanges between Starling and Lecter form the terrifying heart of this Oscar-winning psychological thriller.", "English", 19_000_000));
    castCrews.AddRange([CC(silenceOfLambs.Id, demme.Id, PersonRole.Director), CC(silenceOfLambs.Id, jodieFoster.Id, PersonRole.Cast), CC(silenceOfLambs.Id, anthonyHopkins.Id, PersonRole.Cast)]);
    movieGenres.AddRange([MG(silenceOfLambs.Id, crime), MG(silenceOfLambs.Id, horror), MG(silenceOfLambs.Id, thriller)]);
    reviews.AddRange([Rev(silenceOfLambs.Id, "PsychThriller", "Hopkins only has 16 minutes of screen time and still dominates every scene he's in.", 10), Rev(silenceOfLambs.Id, "FosterFan", "Foster's Clarice is one of cinema's all-time great protagonists.", 10), Rev(silenceOfLambs.Id, "ThrillerTessa", "A rare horror-thriller hybrid that also won Best Picture, and deservedly so.", 9)]);

    // 19 — The Matrix
    var matrix = M("The Matrix", 1999, 3, 31, "A computer programmer discovers that reality is a simulation controlled by machines and joins a rebellion.", 136);
    movies.Add(matrix);
    details.Add(D(matrix.Id, "Thomas Anderson, a computer programmer by day and hacker by night, is contacted by mysterious rebels who reveal that the world he knows is a simulated reality called the Matrix, created by machines to distract humans while using their bodies as an energy source. He must choose to take a red or blue pill and discover the truth.", "English", 63_000_000));
    castCrews.AddRange([CC(matrix.Id, wachowski.Id, PersonRole.Director), CC(matrix.Id, keanuReeves.Id, PersonRole.Cast), CC(matrix.Id, fishburne.Id, PersonRole.Cast), CC(matrix.Id, carrieAnneMoss.Id, PersonRole.Cast)]);
    movieGenres.AddRange([MG(matrix.Id, action), MG(matrix.Id, scifi)]);
    reviews.AddRange([Rev(matrix.Id, "RedPill", "Revolutionary in every sense of the word. Changed action and sci-fi forever.", 10), Rev(matrix.Id, "BulletTime", "Bullet-time, philosophy, kung-fu and Keanu Reeves. What more could you want?", 9), RevBy(matrix.Id, marcus, "Changed how action movies looked and felt overnight. Still holds up flawlessly.", 10)]);

    // 20 — Forrest Gump
    var forrestGump = M("Forrest Gump", 1994, 7, 6, "A kind-hearted man from Alabama unwittingly influences several major historical events across several decades.", 142);
    movies.Add(forrestGump);
    details.Add(D(forrestGump.Id, "The presidencies of Kennedy and Johnson, the events of Vietnam, Watergate and other history unfold through the perspective of an Alabama man with an IQ of 75. Though simple-minded, Forrest Gump's unwavering love for his childhood sweetheart Jenny, his mother's wisdom, and his accidental involvement in defining moments of history make him a truly extraordinary everyman.", "English", 55_000_000));
    castCrews.AddRange([CC(forrestGump.Id, zemeckis.Id, PersonRole.Director), CC(forrestGump.Id, ericRoth.Id, PersonRole.Writer), CC(forrestGump.Id, tomHanks.Id, PersonRole.Cast), CC(forrestGump.Id, nataliePortman.Id, PersonRole.Cast)]);
    movieGenres.AddRange([MG(forrestGump.Id, drama), MG(forrestGump.Id, romance)]);
    reviews.AddRange([Rev(forrestGump.Id, "HeartWarmer", "Life is like a box of chocolates — this film is the best one in the box.", 10), Rev(forrestGump.Id, "TomHanksFan", "Hanks gives the performance of a lifetime. A genuinely moving American epic.", 10), Rev(forrestGump.Id, "AmericanaAndy", "Simple storytelling that somehow captures decades of American history with real heart.", 9)]);

    // 21 — The Shawshank Redemption
    var shawshank = M("The Shawshank Redemption", 1994, 9, 23, "A wrongly convicted banker forms an unlikely friendship with a fellow inmate over decades in Shawshank State Penitentiary.", 142);
    movies.Add(shawshank);
    details.Add(D(shawshank.Id, "Andy Dufresne, a mild-mannered banker, is sentenced to life in Shawshank State Penitentiary for a murder he did not commit. Over the course of two decades, he forms a profound friendship with fellow inmate Red and quietly maintains his hope and dignity in the face of brutal injustice, working toward an ingenious escape.", "English", 25_000_000));
    castCrews.AddRange([CC(shawshank.Id, darabont.Id, PersonRole.Director), CC(shawshank.Id, timRobbins.Id, PersonRole.Cast), CC(shawshank.Id, freeman.Id, PersonRole.Cast)]);
    movieGenres.AddRange([MG(shawshank.Id, drama), MG(shawshank.Id, crime)]);
    reviews.AddRange([RevBy(shawshank.Id, priya, "A story about hope that never once feels sentimental. Robbins and Freeman are extraordinary together.", 10), Rev(shawshank.Id, "PrisonDrama", "Consistently ranked the greatest film ever made for good reason. Every scene earns its emotional weight.", 10), Rev(shawshank.Id, "SlowBurnSam", "Patient, humane storytelling. The final act still gets me every time.", 9)]);

    // 22 — Pulp Fiction
    var pulpFiction = M("Pulp Fiction", 1994, 10, 14, "Interweaving tales of Los Angeles criminals, from hitmen and a boxer to a gangster's wife and a pair of diner bandits.", 154);
    movies.Add(pulpFiction);
    details.Add(D(pulpFiction.Id, "Vincent Vega and Jules Winnfield, a pair of philosophical hitmen, cross paths with a boxer paid to throw a fight, a gangster's wife, and a duo of diner bandits in this nonlinear crime anthology. Tarantino's genre-bending mix of pulpy violence, black comedy, and pop-culture dialogue redefined independent cinema in the 1990s.", "English", 8_000_000));
    castCrews.AddRange([CC(pulpFiction.Id, tarantino.Id, PersonRole.Director), CC(pulpFiction.Id, johnTravolta.Id, PersonRole.Cast), CC(pulpFiction.Id, samuelLJackson.Id, PersonRole.Cast), CC(pulpFiction.Id, umaThurman.Id, PersonRole.Cast)]);
    movieGenres.AddRange([MG(pulpFiction.Id, crime), MG(pulpFiction.Id, drama)]);
    reviews.AddRange([RevBy(pulpFiction.Id, marcus, "Reinvented what a crime film could be. The dialogue alone is a masterclass in screenwriting.", 10), Rev(pulpFiction.Id, "NonlinearNick", "Endlessly quotable and structurally brilliant. Tarantino's best work.", 10), Rev(pulpFiction.Id, "IndieFilmFan", "The needle-drop soundtrack and Jackson's Ezekiel speech alone make this essential viewing.", 9)]);

    // 23 — The Departed
    var departed = M("The Departed", 2006, 10, 6, "An undercover cop and a mole in the police force try to identify each other while infiltrating the Irish mob in Boston.", 151);
    movies.Add(departed);
    details.Add(D(departed.Id, "To take down South Boston's Irish mob, the police send in Billy Costigan to infiltrate the organization run by Frank Costello, while Colin Sullivan, a mole in the police department, works for the mob. When it becomes clear the department has a spy, and the syndicate has one as well, both sides scramble to root out the traitor before they're exposed.", "English", 90_000_000));
    castCrews.AddRange([CC(departed.Id, scorsese.Id, PersonRole.Director), CC(departed.Id, diCaprio.Id, PersonRole.Cast), CC(departed.Id, mattDamon.Id, PersonRole.Cast), CC(departed.Id, jackNicholson.Id, PersonRole.Cast)]);
    movieGenres.AddRange([MG(departed.Id, crime), MG(departed.Id, thriller), MG(departed.Id, drama)]);
    reviews.AddRange([Rev(departed.Id, "BostonBorn", "Scorsese finally got his Oscar and it's well deserved. Relentlessly tense from start to finish.", 9), Rev(departed.Id, "CrimeSagaCarl", "DiCaprio and Damon at their absolute best, playing cat and mouse across a shifting moral line.", 9), Rev(departed.Id, "TwistEnding", "That final shot still shocks me no matter how many times I watch it.", 8)]);

    // 24 — Whiplash
    var whiplash = M("Whiplash", 2014, 10, 10, "A driven young drummer enrolls at a cutthroat music conservatory under the abusive instruction of a ruthless bandleader.", 106);
    movies.Add(whiplash);
    details.Add(D(whiplash.Id, "Andrew Neiman, an ambitious young jazz drummer, enrolls at a cutthroat New York music conservatory where his dreams of greatness are mentored by an instructor who will stop at nothing to realize a student's potential, including verbal and physical abuse. The film's punishing rhythm mirrors the drumming at its core.", "English", 3_300_000));
    castCrews.AddRange([CC(whiplash.Id, chazelle.Id, PersonRole.Director), CC(whiplash.Id, milesTeller.Id, PersonRole.Cast), CC(whiplash.Id, jkSimmons.Id, PersonRole.Cast)]);
    movieGenres.AddRange([MG(whiplash.Id, drama)]);
    reviews.AddRange([Rev(whiplash.Id, "DrumRoll", "J.K. Simmons is terrifying. The final performance is one of the most intense sequences in modern film.", 10), Rev(whiplash.Id, "JazzHead", "Editing this tight and this tense around a drum kit shouldn't be possible, but here we are.", 9), Rev(whiplash.Id, "MusicCritic22", "Punishing, exhilarating, and impossible to look away from.", 9)]);

    // 25 — La La Land
    var laLaLand = M("La La Land", 2016, 12, 9, "An aspiring actress and a dedicated jazz musician fall in love while pursuing their dreams in Los Angeles.", 128);
    movies.Add(laLaLand);
    details.Add(D(laLaLand.Id, "Mia, an aspiring actress, and Sebastian, a dedicated jazz musician, struggle to make ends meet while chasing their dreams in a city known for crushing hopes. As success starts to fall into place, the couple must face the possibility that their romance and their ambitions may not be able to coexist.", "English", 30_000_000));
    castCrews.AddRange([CC(laLaLand.Id, chazelle.Id, PersonRole.Director), CC(laLaLand.Id, emmaStone.Id, PersonRole.Cast), CC(laLaLand.Id, ryanGosling.Id, PersonRole.Cast)]);
    movieGenres.AddRange([MG(laLaLand.Id, romance), MG(laLaLand.Id, drama), MG(laLaLand.Id, comedy)]);
    reviews.AddRange([Rev(laLaLand.Id, "MusicalMike", "A gorgeous throwback to old Hollywood musicals with a bittersweet modern heart.", 9), Rev(laLaLand.Id, "DreamerDana", "That ending sequence is one of the most beautifully devastating things I've seen in a theater.", 10), Rev(laLaLand.Id, "JazzHead", "Gosling and Stone have effortless chemistry. The Griffith Observatory scene is pure magic.", 8)]);

    // 26 — Parasite
    var parasite = M("Parasite", 2019, 5, 30, "Greed and class discrimination threaten a newly formed symbiotic relationship between a wealthy family and a destitute one.", 132);
    movies.Add(parasite);
    details.Add(D(parasite.Id, "All unemployed, the Kim family takes an interest in the wealthy Park family for their livelihood, scheming to become indispensable to them by infiltrating their household as unrelated, highly qualified employees. A devious plan sets in motion a chain of events that spirals into darkly comic and shocking territory.", "Korean", 11_400_000));
    castCrews.AddRange([CC(parasite.Id, bongJoonHo.Id, PersonRole.Director), CC(parasite.Id, songKangHo.Id, PersonRole.Cast), CC(parasite.Id, choiWooShik.Id, PersonRole.Cast)]);
    movieGenres.AddRange([MG(parasite.Id, thriller), MG(parasite.Id, drama), MG(parasite.Id, comedy)]);
    reviews.AddRange([RevBy(parasite.Id, alice, "A masterclass in tone control - hilarious, tense, and devastating, sometimes in the same scene.", 10), Rev(parasite.Id, "GlobalCinemaFan", "Deserved every single Oscar it won. A perfect screenplay from top to bottom.", 10), Rev(parasite.Id, "SocialCommentary", "The basement reveal is one of the great mid-film turns in recent memory.", 9)]);

    // 27 — Get Out
    var getOut = M("Get Out", 2017, 2, 24, "A young Black man uncovers a disturbing secret when he meets his white girlfriend's family for the first time.", 104);
    movies.Add(getOut);
    details.Add(D(getOut.Id, "Chris and his girlfriend Rose go upstate to visit her parents for the weekend, where his unease about their overly accommodating behavior gives way to a nightmarish realization involving hypnosis and a horrifying family secret. Jordan Peele's directorial debut fuses social horror with sharp satire.", "English", 4_500_000));
    castCrews.AddRange([CC(getOut.Id, jordanPeele.Id, PersonRole.Director), CC(getOut.Id, danielKaluuya.Id, PersonRole.Cast), CC(getOut.Id, allisonWilliams.Id, PersonRole.Cast)]);
    movieGenres.AddRange([MG(getOut.Id, horror), MG(getOut.Id, thriller)]);
    reviews.AddRange([RevBy(getOut.Id, ben, "Smart, unsettling, and says something real underneath all the horror-movie mechanics.", 9), Rev(getOut.Id, "SocialThriller", "The sunken place sequence alone cements this as a modern horror classic.", 9), Rev(getOut.Id, "GenreBender", "Peele's command of tone from the opening scene onward is remarkable for a debut.", 8)]);

    // 28 — Mad Max: Fury Road
    var madMax = M("Mad Max: Fury Road", 2015, 5, 15, "In a post-apocalyptic wasteland, a woman rebels against a tyrannical ruler in search of her home with the aid of a drifter.", 120);
    movies.Add(madMax);
    details.Add(D(madMax.Id, "In a desert wasteland where humanity is broken, Max joins forces with the rebellious Imperator Furiosa to flee from a cult leader who controls the region's water supply. Told through a nearly continuous chase across the wasteland, the film reinvents the action genre through practical stunts and relentless momentum.", "English", 154_600_000));
    castCrews.AddRange([CC(madMax.Id, georgeMiller.Id, PersonRole.Director), CC(madMax.Id, tomHardy.Id, PersonRole.Cast), CC(madMax.Id, charlizeTheron.Id, PersonRole.Cast)]);
    movieGenres.AddRange([MG(madMax.Id, action), MG(madMax.Id, adventure), MG(madMax.Id, scifi)]);
    reviews.AddRange([RevBy(madMax.Id, ben, "Non-stop practical action filmmaking at its absolute peak. Astonishing stunt work throughout.", 10), Rev(madMax.Id, "PracticalFX", "Proof that you don't need CGI overload to make an unforgettable action movie.", 10), Rev(madMax.Id, "WastelandWanderer", "Furiosa is one of the best action heroes of the decade. Relentless and gorgeous.", 9)]);

    // 29 — No Country for Old Men
    var noCountry = M("No Country for Old Men", 2007, 11, 9, "A hunter stumbles upon a drug deal gone wrong and the ensuing pursuit by a remorseless killer across the Texas desert.", 122);
    movies.Add(noCountry);
    details.Add(D(noCountry.Id, "Llewelyn Moss stumbles upon dead bodies, a stash of heroin, and more than two million dollars in cash near the Rio Grande. Moss's find sets off a chain reaction of violence, as a remorseless killer named Anton Chigurh pursues him across the desert, while an aging sheriff tries to make sense of a world growing more brutal than he can comprehend.", "English", 25_000_000));
    castCrews.AddRange([CC(noCountry.Id, joelCoen.Id, PersonRole.Director), CC(noCountry.Id, ethanCoen.Id, PersonRole.Writer), CC(noCountry.Id, javierBardem.Id, PersonRole.Cast), CC(noCountry.Id, joshBrolin.Id, PersonRole.Cast), CC(noCountry.Id, tommyLeeJones.Id, PersonRole.Cast)]);
    movieGenres.AddRange([MG(noCountry.Id, crime), MG(noCountry.Id, thriller), MG(noCountry.Id, drama)]);
    reviews.AddRange([RevBy(noCountry.Id, priya, "Bardem's Chigurh is one of the most terrifying villains ever put on screen. Bleak and masterful.", 10), Rev(noCountry.Id, "NeoWesternNed", "The Coens at the height of their control over tone and tension.", 10), Rev(noCountry.Id, "DesertNoir", "No music, no mercy. A masterclass in sustained dread.", 9)]);

    // 30 — There Will Be Blood
    var thereWillBeBlood = M("There Will Be Blood", 2007, 12, 26, "A ruthless silver miner turned oilman builds an empire in early twentieth-century California, consumed by greed and ambition.", 158);
    movies.Add(thereWillBeBlood);
    details.Add(D(thereWillBeBlood.Id, "Ruthless silver miner-turned-oilman Daniel Plainview moves to a small California community to drill for oil after his son is injured in an accident. As his wealth and ambition grow, so does his contempt for those around him, including a young preacher who becomes his greatest rival, in a sprawling story of greed, faith, and isolation.", "English", 25_000_000));
    castCrews.AddRange([CC(thereWillBeBlood.Id, ptAnderson.Id, PersonRole.Director), CC(thereWillBeBlood.Id, danielDayLewis.Id, PersonRole.Cast)]);
    movieGenres.AddRange([MG(thereWillBeBlood.Id, drama)]);
    reviews.AddRange([RevBy(thereWillBeBlood.Id, alice, "Day-Lewis gives one of the greatest performances in film history. Utterly consuming.", 10), Rev(thereWillBeBlood.Id, "OilAndAmbition", "A slow-burning American epic about greed that only gets more relevant with time.", 10), Rev(thereWillBeBlood.Id, "PTASuperfan", "The milkshake speech is iconic for a reason. Towering, uncompromising filmmaking.", 9)]);

    // 31 — The Grand Budapest Hotel
    var grandBudapest = M("The Grand Budapest Hotel", 2014, 3, 7, "A legendary concierge and his loyal lobby boy become entangled in a theft and murder involving a wealthy family's fortune.", 99);
    movies.Add(grandBudapest);
    details.Add(D(grandBudapest.Id, "The adventures of Gustave H, a legendary concierge at a famous European hotel between the wars, and Zero Moustafa, the lobby boy who becomes his most trusted friend. Their story involves the theft and recovery of a priceless Renaissance painting and the battle for an enormous family fortune, all told with Wes Anderson's signature symmetrical whimsy.", "English", 25_000_000));
    castCrews.AddRange([CC(grandBudapest.Id, wesAnderson.Id, PersonRole.Director), CC(grandBudapest.Id, ralphFiennes.Id, PersonRole.Cast)]);
    movieGenres.AddRange([MG(grandBudapest.Id, comedy), MG(grandBudapest.Id, adventure), MG(grandBudapest.Id, drama)]);
    reviews.AddRange([Rev(grandBudapest.Id, "SymmetryLover", "Impeccably crafted down to the last frame. Fiennes is hilarious and unexpectedly moving.", 9), Rev(grandBudapest.Id, "PastelPalette", "Wes Anderson's most purely enjoyable film. A candy-colored delight.", 9), Rev(grandBudapest.Id, "EuropeanCharm", "Whimsical on the surface with real melancholy underneath.", 8)]);

    // 32 — Casablanca
    var casablanca = M("Casablanca", 1942, 11, 26, "A cynical American expatriate must choose between his own safety and helping the resistance leader who is his lost love's husband.", 102);
    movies.Add(casablanca);
    details.Add(D(casablanca.Id, "In the early days of WWII, an American expatriate meets a former lover in Casablanca and must choose between his love for her and helping her husband, a Czech resistance leader, escape the Vichy-controlled city to continue his fight against the Nazis. Their reunion forces old wounds and impossible choices to the surface.", "English", 950_000));
    castCrews.AddRange([CC(casablanca.Id, curtiz.Id, PersonRole.Director), CC(casablanca.Id, humphreyBogart.Id, PersonRole.Cast), CC(casablanca.Id, ingridBergman.Id, PersonRole.Cast)]);
    movieGenres.AddRange([MG(casablanca.Id, romance), MG(casablanca.Id, drama)]);
    reviews.AddRange([RevBy(casablanca.Id, priya, "Here's looking at you, kid — still one of the most quoted and beloved films ever made.", 10), Rev(casablanca.Id, "GoldenAgeGal", "Bogart and Bergman have chemistry that time hasn't dulled one bit.", 10), Rev(casablanca.Id, "ClassicHollywood", "Every line of dialogue is quotable. A perfect wartime romance.", 9)]);

    // 33 — Star Wars: A New Hope
    var starWars = M("Star Wars: A New Hope", 1977, 5, 25, "A young farm boy joins a rebellion to rescue a princess and destroy a galactic empire's ultimate weapon.", 121);
    movies.Add(starWars);
    details.Add(D(starWars.Id, "Luke Skywalker joins forces with a Jedi Knight, a cocky pilot, a Wookiee, and two droids to save the galaxy from the Empire's world-destroying battle station, while also attempting to rescue Princess Leia from the mysterious Darth Vader. George Lucas's space opera launched one of the most influential franchises in film history.", "English", 11_000_000));
    castCrews.AddRange([CC(starWars.Id, georgeLucas.Id, PersonRole.Director), CC(starWars.Id, markHamill.Id, PersonRole.Cast), CC(starWars.Id, harrisonFord.Id, PersonRole.Cast), CC(starWars.Id, carrieFisher.Id, PersonRole.Cast)]);
    movieGenres.AddRange([MG(starWars.Id, scifi), MG(starWars.Id, adventure), MG(starWars.Id, action)]);
    reviews.AddRange([RevBy(starWars.Id, marcus, "The film that defined a genre and a generation. Still thrilling nearly fifty years later.", 10), Rev(starWars.Id, "GalaxyFarFar", "Groundbreaking effects and a mythic story structure that just works.", 10), Rev(starWars.Id, "SpaceOperaFan", "The Death Star trench run remains one of cinema's great action climaxes.", 9)]);

    // 34 — Back to the Future
    var backToTheFuture = M("Back to the Future", 1985, 7, 3, "A teenager is accidentally sent thirty years into the past in a time-traveling car and must ensure his parents fall in love.", 116);
    movies.Add(backToTheFuture);
    details.Add(D(backToTheFuture.Id, "Marty McFly is accidentally sent back to 1955 in a time-traveling DeLorean built by his eccentric scientist friend Doc Brown. Stuck in the past, Marty must make sure his high-school-age parents meet and fall in love, or risk erasing his own existence, all while finding a way back to 1985.", "English", 19_000_000));
    castCrews.AddRange([CC(backToTheFuture.Id, zemeckis.Id, PersonRole.Director), CC(backToTheFuture.Id, michaelJFox.Id, PersonRole.Cast), CC(backToTheFuture.Id, christopherLloyd.Id, PersonRole.Cast)]);
    movieGenres.AddRange([MG(backToTheFuture.Id, scifi), MG(backToTheFuture.Id, adventure), MG(backToTheFuture.Id, comedy)]);
    reviews.AddRange([Rev(backToTheFuture.Id, "EightyEightMPH", "A perfectly constructed time-travel comedy. Not a single wasted scene.", 10), Rev(backToTheFuture.Id, "FluxCapacitor", "Endlessly rewatchable. Fox and Lloyd have incredible buddy chemistry.", 9), Rev(backToTheFuture.Id, "RetroRewind", "Great Scott, this movie still holds up completely.", 9)]);

    // 35 — Toy Story
    var toyStory = M("Toy Story", 1995, 11, 22, "A cowboy doll's world is turned upside down when a spaceman action figure becomes his owner's new favorite toy.", 81);
    movies.Add(toyStory);
    details.Add(D(toyStory.Id, "A cowboy doll named Woody is Andy's favorite toy until a fancy new spaceman figure, Buzz Lightyear, arrives and threatens to replace him. When the two are separated from their owner during a move, they must learn to work together to find their way home, in Pixar's first feature-length film.", "English", 30_000_000));
    castCrews.AddRange([CC(toyStory.Id, lasseter.Id, PersonRole.Director), CC(toyStory.Id, tomHanks.Id, PersonRole.Cast), CC(toyStory.Id, timAllen.Id, PersonRole.Cast)]);
    movieGenres.AddRange([MG(toyStory.Id, animation), MG(toyStory.Id, comedy), MG(toyStory.Id, adventure)]);
    reviews.AddRange([RevBy(toyStory.Id, marcus, "The first fully CG feature film and it still holds up as a genuinely great story.", 9), Rev(toyStory.Id, "PixarPioneer", "Launched an entire studio's legacy. Funny, heartfelt, and technically astonishing for its time.", 10), Rev(toyStory.Id, "ToyBoxTales", "Hanks and Allen's chemistry carries the whole film. A landmark in animation.", 9)]);

    // 36 — Spirited Away
    var spiritedAway = M("Spirited Away", 2001, 7, 20, "A young girl wanders into a magical world of spirits and must work in a bathhouse to free herself and her parents.", 125);
    movies.Add(spiritedAway);
    details.Add(D(spiritedAway.Id, "During her family's move to a new neighborhood, ten-year-old Chihiro wanders into a mysterious world governed by spirits and witches, where her parents are transformed into pigs. To free her family and return home, she must work in a bathhouse for spirits, guided by the enigmatic Haku, in Hayao Miyazaki's beloved animated masterpiece.", "Japanese", 19_000_000));
    castCrews.AddRange([CC(spiritedAway.Id, miyazaki.Id, PersonRole.Director), CC(spiritedAway.Id, miyazaki.Id, PersonRole.Writer)]);
    movieGenres.AddRange([MG(spiritedAway.Id, animation), MG(spiritedAway.Id, adventure), MG(spiritedAway.Id, drama)]);
    reviews.AddRange([Rev(spiritedAway.Id, "AnimeAppreciator", "Miyazaki's most fully realized fantasy world. Endlessly inventive from start to finish.", 10), Rev(spiritedAway.Id, "GhibliFan", "A beautiful, strange, and moving coming-of-age story unlike anything else in animation.", 10), Rev(spiritedAway.Id, "MagicalRealism", "The bathhouse setting alone is worth the price of admission. Gorgeous hand-drawn animation.", 9)]);

    // 37 — The Green Mile
    var greenMile = M("The Green Mile", 1999, 12, 10, "A death row corrections officer discovers a mysterious inmate possesses a miraculous, healing gift.", 189);
    movies.Add(greenMile);
    details.Add(D(greenMile.Id, "Paul Edgecomb, a death row corrections officer during the Great Depression, witnesses supernatural events after the arrival of John Coffey, a Black man convicted of a brutal crime who possesses an extraordinary healing gift. As Paul comes to doubt Coffey's guilt, he must reconcile the man's power with the machinery of justice set against him.", "English", 60_000_000));
    castCrews.AddRange([CC(greenMile.Id, darabont.Id, PersonRole.Director), CC(greenMile.Id, tomHanks.Id, PersonRole.Cast), CC(greenMile.Id, michaelClarkeDuncan.Id, PersonRole.Cast)]);
    movieGenres.AddRange([MG(greenMile.Id, drama), MG(greenMile.Id, crime)]);
    reviews.AddRange([Rev(greenMile.Id, "DeathRowDrama", "Devastating and tender in equal measure. Duncan's performance breaks your heart.", 10), Rev(greenMile.Id, "MagicalRealism", "A long runtime that earns every minute of its emotional payoff.", 9), Rev(greenMile.Id, "PrisonDrama", "Darabont proves lightning can strike twice with a prison-set drama.", 9)]);

    // 38 — Saving Private Ryan
    var savingPrivateRyan = M("Saving Private Ryan", 1998, 7, 24, "A squad of soldiers is sent behind enemy lines to retrieve a paratrooper whose brothers have all been killed in action.", 169);
    movies.Add(savingPrivateRyan);
    details.Add(D(savingPrivateRyan.Id, "Following the brutal opening assault on Omaha Beach, a group of U.S. soldiers led by Captain Miller is sent on a dangerous mission behind enemy lines: to find and bring home a paratrooper whose three brothers have all been killed in action, questioning the value of one life against the cost of the mission itself.", "English", 70_000_000));
    castCrews.AddRange([CC(savingPrivateRyan.Id, spielberg.Id, PersonRole.Director), CC(savingPrivateRyan.Id, tomHanks.Id, PersonRole.Cast), CC(savingPrivateRyan.Id, mattDamon.Id, PersonRole.Cast)]);
    movieGenres.AddRange([MG(savingPrivateRyan.Id, action), MG(savingPrivateRyan.Id, drama)]);
    reviews.AddRange([Rev(savingPrivateRyan.Id, "WarFilmBuff", "The Omaha Beach sequence redefined how war is depicted on screen.", 10), Rev(savingPrivateRyan.Id, "HistoryGeek", "Visceral, harrowing, and deeply human amid the chaos.", 10), Rev(savingPrivateRyan.Id, "TomHanksFan", "Hanks anchors an ensemble of remarkable performances.", 9)]);

    // 39 — Django Unchained
    var django = M("Django Unchained", 2012, 12, 25, "A freed slave teams with a bounty hunter to rescue his wife from a ruthless Mississippi plantation owner.", 165);
    movies.Add(django);
    details.Add(D(django.Id, "With the help of a German bounty hunter, a freed slave named Django sets out to rescue his wife from a brutal Mississippi plantation owner, adopting the bounty hunting trade along the way. Tarantino's revisionist Western confronts American slavery through his trademark blend of stylized violence and sharp dialogue.", "English", 100_000_000));
    castCrews.AddRange([CC(django.Id, tarantino.Id, PersonRole.Director), CC(django.Id, jamieFoxx.Id, PersonRole.Cast), CC(django.Id, christophWaltz.Id, PersonRole.Cast), CC(django.Id, diCaprio.Id, PersonRole.Cast)]);
    movieGenres.AddRange([MG(django.Id, action), MG(django.Id, drama), MG(django.Id, crime)]);
    reviews.AddRange([RevBy(django.Id, marcus, "Waltz steals every scene he's in. A bold, brutal, and blackly funny Western.", 9), Rev(django.Id, "BountyHunterBen", "DiCaprio's villain is one of the most purely hateable characters he's ever played.", 9), Rev(django.Id, "SpaghettiWesternFan", "Tarantino's love of genre filmmaking is on full, gleeful display.", 8)]);

    // 40 — Amadeus
    var amadeus = M("Amadeus", 1984, 9, 19, "A rival composer becomes consumed by jealousy over the brilliance of the young Wolfgang Amadeus Mozart.", 160);
    movies.Add(amadeus);
    details.Add(D(amadeus.Id, "Told in flashback from an asylum, aging composer Antonio Salieri recounts his obsessive rivalry with the young, boorish, and undeniably brilliant Wolfgang Amadeus Mozart. As Salieri's envy curdles into a scheme against the man he considers touched by God, the film becomes a meditation on genius, mediocrity, and faith.", "English", 18_000_000));
    castCrews.AddRange([CC(amadeus.Id, forman.Id, PersonRole.Director), CC(amadeus.Id, murrayAbraham.Id, PersonRole.Cast), CC(amadeus.Id, tomHulce.Id, PersonRole.Cast)]);
    movieGenres.AddRange([MG(amadeus.Id, drama)]);
    reviews.AddRange([Rev(amadeus.Id, "ClassicalComposer", "Abraham's Salieri is one of the great tortured performances in film history.", 10), Rev(amadeus.Id, "OperaBuff", "Gorgeous, funny, and tragic in equal measure. A criminally underseen masterpiece today.", 9), Rev(amadeus.Id, "PeriodPieceFan", "The costumes and music alone make this a feast, but the performances seal it.", 9)]);

    db.Movies.AddRange(movies);
    db.MovieDetails.AddRange(details);
    db.CastCrews.AddRange(castCrews);
    db.MovieGenres.AddRange(movieGenres);
    db.Reviews.AddRange(reviews);

    await db.SaveChangesAsync();

    logger.LogInformation(
        "Seeded {MovieCount} movies, {PersonCount} persons, {GenreCount} genres, {ReviewCount} reviews, {UserCount} example users.",
        movies.Count, db.Persons.Local.Count, db.Genres.Local.Count, reviews.Count, 4);
  }
}
