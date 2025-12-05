using System;
using System.Linq;
using FoutloosTypen.Core.Data.Repositories;
using FoutloosTypen.Core.Models;
using NUnit.Framework;

namespace TestCore
{
    [TestFixture]
    public class Test_UnitTest_RealData
    {
        private LessonRepository? _lessonRepository;
        private PracticeMaterialRepository? _practiceMaterialRepository;

        [SetUp]
        public void Setup()
        {
            _lessonRepository = new LessonRepository();
            _practiceMaterialRepository = new PracticeMaterialRepository();
        }

        [TearDown]
        public void TearDown()
        {
            _lessonRepository?.Dispose();
            _practiceMaterialRepository?.Dispose();
        }


        /// Test 1: Controleert of GetAll() lessen teruggeeft
        [Test]
        public void LessonRepository_GetAll_ShouldReturnLessons()
        {
            // Act
            var lessons = _lessonRepository!.GetAll();

            // Assert
            Assert.That(lessons, Is.Not.Null, "De lijst met lessen mag niet null zijn");
            Assert.That(lessons.Count, Is.GreaterThan(0), "Er moeten lessen aanwezig zijn");
        }

        /// Test 2: Controleert of er exact 30 lessen zijn (3 cursussen x 10 lessen)
        [Test]
        public void LessonRepository_GetAll_ShouldReturn30Lessons()
        {
            // Act
            var lessons = _lessonRepository!.GetAll();

            // Assert
            Assert.That(lessons.Count, Is.EqualTo(30), "Er moeten exact 30 lessen zijn");
        }

        /// Test 3: Controleert of Get() een specifieke les kan ophalen
        [Test]
        public void LessonRepository_Get_ShouldReturnSpecificLesson()
        {
            // Arrange
            int lessonId = 1;

            // Act
            var lesson = _lessonRepository!.Get(lessonId);

            // Assert
            Assert.That(lesson, Is.Not.Null, "Les moet gevonden worden");
            Assert.That(lesson.Id, Is.EqualTo(lessonId), "Les ID moet kloppen");
            Assert.That(lesson.Name, Is.Not.Empty, "Les moet een naam hebben");
        }

        /// Test 4: Controleert of elke cursus 10 lessen heeft
        [Test]
        public void LessonRepository_GetAll_EachCourseShouldHave10Lessons()
        {
            // Act
            var lessons = _lessonRepository!.GetAll();
            var course1Lessons = lessons.Where(l => l.CourseId == 1).ToList();
            var course2Lessons = lessons.Where(l => l.CourseId == 2).ToList();
            var course3Lessons = lessons.Where(l => l.CourseId == 3).ToList();

            // Assert
            Assert.That(course1Lessons.Count, Is.EqualTo(10), "Cursus 1 moet 10 lessen hebben");
            Assert.That(course2Lessons.Count, Is.EqualTo(10), "Cursus 2 moet 10 lessen hebben");
            Assert.That(course3Lessons.Count, Is.EqualTo(10), "Cursus 3 moet 10 lessen hebben");
        }

        /// Test 5: Controleert of alle lessen 60 seconden duren
        [Test]
        public void LessonRepository_GetAll_AllLessonsShouldHave60SecondsDuration()
        {
            // Act
            var lessons = _lessonRepository!.GetAll();

            // Assert
            Assert.That(lessons, Is.All.Matches<Lesson>(l => l.TotalTime == 60),
                "Alle lessen moeten 60 seconden duren");
        }


        /// Test 6: Controleert of alle lessen geldige properties hebben
        [Test]
        public void LessonRepository_GetAll_AllLessonsShouldHaveValidProperties()
        {
            // Act
            var lessons = _lessonRepository!.GetAll();

            // Assert
            foreach (var lesson in lessons)
            {
                Assert.That(lesson.Id, Is.GreaterThan(0), $"Les {lesson.Name}: ID moet groter zijn dan 0");
                Assert.That(lesson.Name, Is.Not.Null.And.Not.Empty, $"Les {lesson.Id}: Naam mag niet leeg zijn");
                Assert.That(lesson.Description, Is.Not.Null.And.Not.Empty, $"Les {lesson.Id}: Beschrijving mag niet leeg zijn");
                Assert.That(lesson.CourseId, Is.InRange(1, 3), $"Les {lesson.Id}: CourseId moet tussen 1 en 3 zijn");
                Assert.That(lesson.IsDone, Is.False, $"Les {lesson.Id}: IsDone moet false zijn bij initialisatie");
            }
        }

        /// Test 7: Controleert of Get() null teruggeeft voor een niet-bestaande les
        [Test]
        public void LessonRepository_Get_ShouldReturnNullForNonExistentLesson()
        {
            // Arrange
            int nonExistentId = 9999;

            // Act
            var lesson = _lessonRepository!.Get(nonExistentId);

            // Assert
            Assert.That(lesson, Is.Null, "Get() moet null teruggeven voor een niet-bestaande les");
        }


        /// Test 8: Controleert of GetAll() practice materials teruggeeft
        [Test]
        public void PracticeMaterialRepository_GetAll_ShouldReturnMaterials()
        {
            // Act
            var materials = _practiceMaterialRepository!.GetAll();

            // Assert
            Assert.That(materials, Is.Not.Null, "De lijst met materials mag niet null zijn");
            Assert.That(materials.Count, Is.GreaterThan(0), "Er moeten practice materials aanwezig zijn");
        }


        /// Test 9: Controleert of Get() een specifiek practice material kan ophalen
        [Test]
        public void PracticeMaterialRepository_Get_ShouldReturnSpecificMaterial()
        {
            // Arrange
            int materialId = 1;

            // Act
            var material = _practiceMaterialRepository!.Get(materialId);

            // Assert
            Assert.That(material, Is.Not.Null, "Material moet gevonden worden");
            Assert.That(material.Id, Is.EqualTo(materialId), "Material ID moet kloppen");
            Assert.That(material.Sentence, Is.Not.Empty, "Material moet een zin hebben");
            Assert.That(material.AssignmentId, Is.GreaterThan(0), "Material moet gekoppeld zijn aan een assignment");
        }

        /// Test 10: Controleert of alle practice materials een zin hebben
        [Test]
        public void PracticeMaterialRepository_GetAll_AllMaterialsShouldHaveSentences()
        {
            // Act
            var materials = _practiceMaterialRepository!.GetAll();

            // Assert
            Assert.That(materials, Is.All.Matches<PracticeMaterial>(m => !string.IsNullOrEmpty(m.Sentence)),
                "Alle practice materials moeten een niet-lege zin hebben");
        }



        /// Test 11: Controleert of alle AssignmentIds geldig zijn (1-150)
        [Test]
        public void PracticeMaterialRepository_GetAll_AllAssignmentIdsShouldBeValid()
        {
            // Act
            var materials = _practiceMaterialRepository!.GetAll();

            // Assert
            foreach (var material in materials)
            {
                Assert.That(material.AssignmentId, Is.InRange(1, 150),
                    $"Material {material.Id}: AssignmentId moet tussen 1 en 150 zijn");
            }
        }

        /// Test 12: Controleert of Get() null teruggeeft voor een niet-bestaand material
        [Test]
        public void PracticeMaterialRepository_Get_ShouldReturnNullForNonExistentMaterial()
        {
            // Arrange
            int nonExistentId = 9999;

            // Act
            var material = _practiceMaterialRepository!.Get(nonExistentId);

            // Assert
            Assert.That(material, Is.Null, "Get() moet null teruggeven voor een niet-bestaand material");
        }
    }
}