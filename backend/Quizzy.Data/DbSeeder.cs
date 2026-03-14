using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Quizzy.Data.Models;

namespace Quizzy.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db, IConfiguration config)
    {
        await db.Database.MigrateAsync();

        if (await db.Users.AnyAsync())
            return; // Already seeded

        // Seed admin
        var adminUsername = config["Admin:Username"] ?? "admin";
        var adminPassword = config["Admin:Password"] ?? "Admin123!";

        var admin = new User
        {
            Username = adminUsername,
            Email = "admin@quizzy.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
            Role = UserRole.Admin
        };
        db.Users.Add(admin);
        await db.SaveChangesAsync();

        // Seed quizzes
        await SeedEasyQuiz(db);
        await SeedMediumQuiz(db);
        await SeedHardQuiz(db);
    }

    private static async Task SeedEasyQuiz(AppDbContext db)
    {
        var quiz = new Quiz
        {
            Title = "General Knowledge — Easy",
            Description = "Test your basic general knowledge with these beginner-friendly questions!",
            Difficulty = Difficulty.Easy,
            TimeLimitPerQuestion = 30
        };
        db.Quizzes.Add(quiz);
        await db.SaveChangesAsync();

        var questions = new (string text, QuestionType type, string[] options, int correctIndex, string? openAnswer)[]
        {
            ("What is the capital of France?", QuestionType.MultipleChoice, ["Paris", "London", "Berlin", "Madrid"], 0, null),
            ("Which planet is known as the Red Planet?", QuestionType.MultipleChoice, ["Venus", "Mars", "Jupiter", "Saturn"], 1, null),
            ("How many continents are there on Earth?", QuestionType.MultipleChoice, ["5", "6", "7", "8"], 2, null),
            ("What is the largest ocean on Earth?", QuestionType.MultipleChoice, ["Atlantic Ocean", "Indian Ocean", "Pacific Ocean", "Arctic Ocean"], 2, null),
            ("Which animal is known as the King of the Jungle?", QuestionType.MultipleChoice, ["Tiger", "Lion", "Elephant", "Bear"], 1, null),
            ("What color do you get when you mix red and white?", QuestionType.MultipleChoice, ["Orange", "Purple", "Pink", "Brown"], 2, null),
            ("What is the chemical symbol for water?", QuestionType.OpenText, [], 0, "H2O"),
            ("Name the closest star to Earth.", QuestionType.OpenText, [], 0, "The Sun"),
            ("What gas do plants absorb from the atmosphere?", QuestionType.OpenText, [], 0, "Carbon dioxide"),
            ("What is the frozen form of water called?", QuestionType.OpenText, [], 0, "Ice"),
        };

        for (int i = 0; i < questions.Length; i++)
        {
            var (text, type, options, correctIndex, openAnswer) = questions[i];
            var question = new Question
            {
                QuizId = quiz.Id,
                Text = text,
                Type = type,
                OrderIndex = i,
                Points = 10
            };
            db.Questions.Add(question);
            await db.SaveChangesAsync();

            if (type == QuestionType.MultipleChoice)
            {
                for (int j = 0; j < options.Length; j++)
                {
                    db.AnswerOptions.Add(new AnswerOption
                    {
                        QuestionId = question.Id,
                        Text = options[j],
                        IsCorrect = j == correctIndex
                    });
                }
            }
            else
            {
                db.OpenTextAnswers.Add(new OpenTextAnswer
                {
                    QuestionId = question.Id,
                    Text = openAnswer!,
                    SimilarityThreshold = 0.6
                });
            }
            await db.SaveChangesAsync();
        }
    }

    private static async Task SeedMediumQuiz(AppDbContext db)
    {
        var quiz = new Quiz
        {
            Title = "Science & History — Medium",
            Description = "A mix of science and history questions for intermediate learners.",
            Difficulty = Difficulty.Medium,
            TimeLimitPerQuestion = 25
        };
        db.Quizzes.Add(quiz);
        await db.SaveChangesAsync();

        var questions = new (string text, QuestionType type, string[] options, int correctIndex, string? openAnswer)[]
        {
            ("What is the powerhouse of the cell?", QuestionType.MultipleChoice, ["Nucleus", "Ribosome", "Mitochondria", "Golgi apparatus"], 2, null),
            ("Who painted the Mona Lisa?", QuestionType.MultipleChoice, ["Michelangelo", "Leonardo da Vinci", "Raphael", "Donatello"], 1, null),
            ("What is the speed of light approximately in km/s?", QuestionType.MultipleChoice, ["150,000", "300,000", "450,000", "600,000"], 1, null),
            ("Which element has the atomic number 79?", QuestionType.MultipleChoice, ["Silver", "Gold", "Platinum", "Copper"], 1, null),
            ("In which year did World War II end?", QuestionType.MultipleChoice, ["1943", "1944", "1945", "1946"], 2, null),
            ("What is the process by which plants make their own food?", QuestionType.OpenText, [], 0, "Photosynthesis"),
            ("Name the longest river in the world.", QuestionType.OpenText, [], 0, "The Nile"),
            ("What is the theory proposed by Charles Darwin?", QuestionType.OpenText, [], 0, "Theory of evolution by natural selection"),
            ("What force keeps us on the ground?", QuestionType.OpenText, [], 0, "Gravity"),
            ("What is the chemical formula for table salt?", QuestionType.OpenText, [], 0, "NaCl"),
        };

        for (int i = 0; i < questions.Length; i++)
        {
            var (text, type, options, correctIndex, openAnswer) = questions[i];
            var question = new Question
            {
                QuizId = quiz.Id,
                Text = text,
                Type = type,
                OrderIndex = i,
                Points = 10
            };
            db.Questions.Add(question);
            await db.SaveChangesAsync();

            if (type == QuestionType.MultipleChoice)
            {
                for (int j = 0; j < options.Length; j++)
                {
                    db.AnswerOptions.Add(new AnswerOption
                    {
                        QuestionId = question.Id,
                        Text = options[j],
                        IsCorrect = j == correctIndex
                    });
                }
            }
            else
            {
                db.OpenTextAnswers.Add(new OpenTextAnswer
                {
                    QuestionId = question.Id,
                    Text = openAnswer!,
                    SimilarityThreshold = 0.65
                });
            }
            await db.SaveChangesAsync();
        }
    }

    private static async Task SeedHardQuiz(AppDbContext db)
    {
        var quiz = new Quiz
        {
            Title = "Advanced Trivia — Hard",
            Description = "Challenge yourself with these tough questions across multiple domains!",
            Difficulty = Difficulty.Hard,
            TimeLimitPerQuestion = 20
        };
        db.Quizzes.Add(quiz);
        await db.SaveChangesAsync();

        var questions = new (string text, QuestionType type, string[] options, int correctIndex, string? openAnswer)[]
        {
            ("What is the Heisenberg Uncertainty Principle about?", QuestionType.MultipleChoice,
                ["Energy conservation", "Simultaneous measurement of position and momentum", "Speed of light constancy", "Quantum entanglement"], 1, null),
            ("Which treaty ended the Thirty Years' War?", QuestionType.MultipleChoice,
                ["Treaty of Versailles", "Treaty of Tordesillas", "Peace of Westphalia", "Treaty of Utrecht"], 2, null),
            ("What is the time complexity of merge sort?", QuestionType.MultipleChoice,
                ["O(n)", "O(n log n)", "O(n²)", "O(log n)"], 1, null),
            ("Who formulated the three laws of motion?", QuestionType.MultipleChoice,
                ["Albert Einstein", "Isaac Newton", "Galileo Galilei", "Nikola Tesla"], 1, null),
            ("Explain the concept of entropy in thermodynamics.", QuestionType.OpenText, [], 0,
                "Entropy is a measure of the disorder or randomness in a system. The second law of thermodynamics states that entropy tends to increase over time."),
            ("What is the significance of the Rosetta Stone?", QuestionType.OpenText, [], 0,
                "The Rosetta Stone was key to deciphering Egyptian hieroglyphics because it contained the same text in three scripts: hieroglyphic, demotic, and Greek."),
            ("Describe the double-slit experiment and its implications.", QuestionType.OpenText, [], 0,
                "The double-slit experiment demonstrates wave-particle duality. When particles like electrons pass through two slits, they create an interference pattern, suggesting they behave as waves."),
            ("What is the halting problem in computer science?", QuestionType.OpenText, [], 0,
                "The halting problem asks whether a program can determine if another program will finish running or loop forever. Alan Turing proved it is undecidable."),
            ("Explain what a black hole event horizon is.", QuestionType.OpenText, [], 0,
                "The event horizon is the boundary around a black hole beyond which nothing, not even light, can escape the gravitational pull."),
            ("What is the difference between TCP and UDP?", QuestionType.OpenText, [], 0,
                "TCP is a connection-oriented protocol that ensures reliable, ordered delivery. UDP is connectionless and faster but does not guarantee delivery or order."),
        };

        for (int i = 0; i < questions.Length; i++)
        {
            var (text, type, options, correctIndex, openAnswer) = questions[i];
            var question = new Question
            {
                QuizId = quiz.Id,
                Text = text,
                Type = type,
                OrderIndex = i,
                Points = 10
            };
            db.Questions.Add(question);
            await db.SaveChangesAsync();

            if (type == QuestionType.MultipleChoice)
            {
                for (int j = 0; j < options.Length; j++)
                {
                    db.AnswerOptions.Add(new AnswerOption
                    {
                        QuestionId = question.Id,
                        Text = options[j],
                        IsCorrect = j == correctIndex
                    });
                }
            }
            else
            {
                db.OpenTextAnswers.Add(new OpenTextAnswer
                {
                    QuestionId = question.Id,
                    Text = openAnswer!,
                    SimilarityThreshold = 0.7
                });
            }
            await db.SaveChangesAsync();
        }
    }
}
