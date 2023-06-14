using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using AIS.Entities;
using AIS.Models;
using Aspose.Cells;
using Microsoft.AspNet.Identity;
using Microsoft.Office.Interop.Word;
using static System.Net.Mime.MediaTypeNames;
using Table = Microsoft.Office.Interop.Word.Table;

namespace AIS.Controllers
{
    // GET: Attestations
    [Authorize(Roles = "Преподаватель,Администратор, Заведующий отделением")]
    public class AttestationsController : Controller
    {
        private readonly AISEntities db = new AISEntities();

        public ActionResult Index(int? idTypeAttestation, int? idGroup, int? idDiscipline) //Открытие страницы со списокм аттестаций, с подгрузкой данных в соответсвии с должностью пользователя
        {
            IEnumerable<Attestation> attestations;
            var idCurentUser = Int32.Parse(User.Identity.GetUserId());
            var curentUser = db.Teachers.Find(idCurentUser);

            var typeAttestations = db.TypeAttestation.ToList();
            typeAttestations.Insert(0, new TypeAttestation { Title = "Все", IdTypeAttestation = 0 });

            var group = db.Group.Where(g => g.IdSpeciality == curentUser.IdSpeciality).ToList();
            group.Insert(0, new Group { Title = "Все", IdGroup = 0 });

            var listOfDiscipline = db.DisciplineTeachers.Where(dis => dis.IdTeacher == idCurentUser).ToList();
            var listIdDiscipline = listOfDiscipline.Select(de => de.IdDiscipline);
            var disciplineCurrentUser = db.Discipline.Where(dcu => listIdDiscipline.Contains(dcu.IdDiscipline)).ToList();
            disciplineCurrentUser.Insert(0, new Discipline { Title = "Все", IdDiscipline = 0 });

            attestations = db.Attestation.Include(a => a.Group).Include(a => a.Discipline).Include(a => a.Teachers).Include(a => a.TypeAttestation).Where(a => a.IdTeachers == idCurentUser && a.Deleted != true);


            if (idTypeAttestation != null && idTypeAttestation != 0) // фильтрация аттестаций по типу
            {
                attestations = attestations.Where(a => a.IdTypeAttestation == idTypeAttestation);
            }

            if (idGroup != null && idGroup != 0) // фильтрация аттестаций по типу
            {
                attestations = attestations.Where(a => a.IdGroup == idGroup);
            }

            if (idDiscipline != null && idDiscipline != 0) // фильтрация аттестаций по типу
            {
                attestations = attestations.Where(a => a.IdDiscipline == idDiscipline);
            }


            AttestationListViewModel attestationListViewModel = new AttestationListViewModel
            {
                Attestations = attestations,
                IdCurentUser = idCurentUser,
                TypeAttestations = new SelectList(typeAttestations, "IdTypeAttestation", "Title"),
                Groups = new SelectList(group, "IdGroup", "Title"),
                Disciplines = new SelectList(disciplineCurrentUser, "IdDiscipline", "Title"),
            };

            return View(attestationListViewModel);
        }

        public ActionResult HeadOfAttestation(int? idTypeAttestation, int? idGroup, int? idDiscipline) //Открытие страницы со списокм аттестаций, с подгрузкой данных в соответсвии с должностью пользователя
        {
            IEnumerable<Attestation> attestations;
            var idCurentUser = Int32.Parse(User.Identity.GetUserId());
            var curentUser = db.Teachers.Find(idCurentUser);

            var typeAttestations = db.TypeAttestation.ToList();
            typeAttestations.Insert(0, new TypeAttestation { Title = "Все", IdTypeAttestation = 0 });

            var group = db.Group.Where(g => g.IdSpeciality == curentUser.IdSpeciality).ToList();
            group.Insert(0, new Group { Title = "Все", IdGroup = 0 });

            var disciplineCurrentUser = db.Discipline.ToList();
            disciplineCurrentUser.Insert(0, new Discipline { Title = "Все", IdDiscipline = 0 });

            attestations = db.Attestation.Include(a => a.Group).Include(a => a.Discipline).Include(a => a.Teachers).Include(a => a.TypeAttestation).Where(a => a.Сompleted == true);


            if (idTypeAttestation != null && idTypeAttestation != 0) // фильтрация аттестаций по типу
            {
                attestations = attestations.Where(a => a.IdTypeAttestation == idTypeAttestation);
            }

            if (idGroup != null && idGroup != 0) // фильтрация аттестаций по типу
            {
                attestations = attestations.Where(a => a.IdGroup == idGroup);
            }

            if (idDiscipline != null && idDiscipline != 0) // фильтрация аттестаций по типу
            {
                attestations = attestations.Where(a => a.IdDiscipline == idDiscipline);
            }


            AttestationListViewModel attestationListViewModel = new AttestationListViewModel
            {
                Attestations = attestations,
                IdCurentUser = idCurentUser,
                TypeAttestations = new SelectList(typeAttestations, "IdTypeAttestation", "Title"),
                Groups = new SelectList(group, "IdGroup", "Title"),
                Disciplines = new SelectList(disciplineCurrentUser, "IdDiscipline", "Title"),
            };

            return View(attestationListViewModel);
        }

        // GET: BlockAttestations
        public ActionResult BlockAttestations(int? idAttestations) //Запрос на завершение аттестации
        {
            var attestation = db.Attestation.Find(idAttestations);
            attestation.Сompleted = true;
            db.Entry(attestation).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");

        }

        // GET: GetAttestationVedomost
        public FileResult GetAttestationVedomost(int? idAttestations) //Запрос на формирование ведомости за экзамен
        {

            var attestation = db.Attestation.Find(idAttestations);
            var vedomosti = db.Vedomosti.Where(v => v.IdAttestation == idAttestations).ToList();
            int count = vedomosti.Count;

            //Словарь тегов, и значений, на которые будут заменены теги в шаблоне ведомости
            var items = new Dictionary<string, string>()
            {
                {"<DISCIPLINE>", attestation.Discipline.Title},
                {"<NUMBERCOURCE>", attestation.Group.CourseNumber},
                {"<TITLEGROUP>", attestation.Group.Title},
                {"<SPECIALITY>", attestation.Group.Speciality.Title},
                {"<FIOPREP>", attestation.Teachers.LastName + " " + attestation.Teachers.FirstName+ " " + attestation.Teachers.Patronymic},
                {"<DATE>", attestation.EndDate.ToString("«dd» MMMM yyyy")}
            };

            Microsoft.Office.Interop.Word.Application wordApp = null;
            Document wordDoc;
            Table wordTable;
            try
            {
                wordApp = new Microsoft.Office.Interop.Word.Application();

                object missing = Type.Missing;
                object fileName = HttpContext.Server.MapPath("~/FilesVedomosti/Ведомость экзамен.docx"); //Путь к шаблону ведомости

                wordDoc = wordApp.Documents.Open(ref fileName, ref missing, ref missing, ref missing); //открываем шаблон ведомости

                foreach (var item in items) // Перебор всех тегов и значений словаря, с последующей
                                            // заменой каждого тега на соответствующее для него значение текущей аттестации
                {
                    Find find = wordApp.Selection.Find;
                    find.Text = item.Key;
                    find.Replacement.Text = item.Value;

                    object wrap = WdFindWrap.wdFindContinue;
                    object replace = WdReplace.wdReplaceAll;

                    find.Execute(FindText: Type.Missing,
                        MatchCase: false, MatchWholeWord: false, MatchWildcards: false,
                        MatchSoundsLike: missing, MatchAllWordForms: false, Forward: true,
                        Wrap: wrap, Format: false, ReplaceWith: missing, Replace: replace);

                }

                wordTable = wordDoc.Tables[2]; //Обращение к таблице результатов студентов за экзамен

                //заполняем ячейки таблицы результатами студентов за экзамен
                for (int i = 2; i <= count + 1; i++)
                    for (int j = 1; j <= 5; j++)
                    {
                        var v = vedomosti[i - 2];
                        if (j == 1)
                            wordTable.Cell(i, j).Range.Text = Convert.ToString(i - 1);
                        if (j == 3)
                            wordTable.Cell(i, j).Range.Text = Convert.ToString(v.Student.FirstName + " " + v.Student.LastName + " " + v.Student.Patronymic);

                        //if (j == 4) //// код для последующей модернизации функции формирования ведомостей под различные типы аттестаций 
                        //    wordTable.Cell(i, j).Range.Text = Convert.ToString(v.TheNumberOfPointsForTheExam);

                        if (j == 4)
                        {
                            string finalGradeString = "";
                            if (v.FinalGrade == "5")
                                finalGradeString = " (отлично)";
                            if (v.FinalGrade == "4")
                                finalGradeString = " (хорошо)";
                            if (v.FinalGrade == "3")
                                finalGradeString = " (удовл.)";
                            if (v.FinalGrade == "2")
                                finalGradeString = " (неудовл.)";
                            wordTable.Cell(i, j).Range.Text = v.FinalGrade + finalGradeString;
                        }

                    }
                // имя нового файла ведомости
                object newFile = HttpContext.Server.MapPath("~/FilesVedomosti/Group/Ведомость по " + attestation.Discipline.Title + " группы " + attestation.Group.Title + ".docx");
                wordDoc.SaveAs2(newFile); //сохранить заполненный данными шаблон как новый документ
                wordApp.ActiveDocument.Close(); //закрытие активного документа
                wordApp?.Quit(); //отключение от приложения для работы с документами типа Word

            }
            catch (Exception ex)
            {
                wordApp.ActiveDocument.Close(); //закрытие активного документа
                wordApp?.Quit();
                Console.WriteLine(ex.Message);
            }

            string path = HttpContext.Server.MapPath("~/FilesVedomosti/Group/Ведомость по " + attestation.Discipline.Title + " группы " + attestation.Group.Title + ".docx"); //путь до сохраненной ранее ведомости
            string fileType = "application/word";
            // Имя файла - необязательно. Это то имя файла, которое будет задано скачиваемому файлу
            string file_name = "Ведомость по " + attestation.Discipline.Title + " группы " + attestation.Group.Title + ".docx";

            return File(path, fileType, file_name); //отправка на клиент файла ведомости
        }

        // GET: GetAttestationVedomost
        public FileResult GetAttestationVedomostStudent(int? idAttestations) //Запрос на формирование ведомости за экзамен
        {

            var attestation = db.Attestation.Find(idAttestations);
            var vedomosti = attestation.Vedomosti.ToList();

            string path = HttpContext.Server.MapPath("~/FilesVedomosti/Group/" + attestation.Group.Title);
            string subpath = $"{attestation.Discipline.Title} {attestation.EndDate:dd.MM.yyyy}";


            DirectoryInfo dirInfo = new DirectoryInfo(path);
            if (!dirInfo.Exists)
            {
                dirInfo.Create();
            }
            dirInfo.CreateSubdirectory(subpath);


            var listiIdCriteriAttestation = attestation.Criteria.Select(c => c.IdCriteria).ToList();
            var listCriteriaAttestation = attestation.Criteria.ToList();
            int count = listCriteriaAttestation.Count;

            var listStudentAttestation = attestation.Group.Student.ToList();

            var resStudent = db.StudentResult.Where(rs => listiIdCriteriAttestation.Contains(rs.IdCriteria)).ToList();

            foreach (Student currentStudent in listStudentAttestation)
            {
                var resultStudentGrade = vedomosti.Where(v => v.IdStudent == currentStudent.IdStudent).First();

                StringBuilder stringBuilder = new StringBuilder();

                stringBuilder.Append(resultStudentGrade.FinalGrade);

                if (resultStudentGrade.FinalGrade == "5")
                    stringBuilder.Append(" (отлично)");
                if (resultStudentGrade.FinalGrade == "4")
                    stringBuilder.Append(" (хорошо)");
                if (resultStudentGrade.FinalGrade == "3")
                    stringBuilder.Append(" (удовлетворительно)");
                if (resultStudentGrade.FinalGrade == "2")
                    stringBuilder.Append(" (неудовлетворительно)");

                //Словарь тегов, и значений, на которые будут заменены теги в шаблоне ведомости
                var items = new Dictionary<string, string>()
                {
                    {"<DISCIPLINE>", attestation.Discipline.Title},
                    {"<NUMBERCOURCE>", attestation.Group.CourseNumber},
                    {"<TITLEGROUP>", attestation.Group.Title},
                    {"<SPECIALITY>", attestation.Group.Speciality.Title},
                    {"<FIOPREP>", attestation.Teachers.LastName + " " + attestation.Teachers.FirstName+ " " + attestation.Teachers.Patronymic},
                    {"<FIOSTUD>", currentStudent.LastName + " " + currentStudent.FirstName+ " " + currentStudent.Patronymic},
                    {"<FINALGRADE>", stringBuilder.ToString() },
                    {"<DATE>", attestation.EndDate.ToString("«dd» MMMM yyyy")}
                };

                var resultCurentStudent = resStudent.Where(rs => rs.IdStudent == currentStudent.IdStudent).ToList();

                Microsoft.Office.Interop.Word.Application wordApp = null;
                Document wordDoc;
                Table wordTable;
                try
                {
                    wordApp = new Microsoft.Office.Interop.Word.Application();

                    object missing = Type.Missing;
                    object fileName = HttpContext.Server.MapPath("~/FilesVedomosti/Ведомость экзамен (студент).docx"); //Путь к шаблону ведомости

                    wordDoc = wordApp.Documents.Open(ref fileName, ref missing, ref missing, ref missing); //открываем шаблон ведомости

                    foreach (var item in items) // Перебор всех тегов и значений словаря, с последующей                                              // заменой каждого тега на соответствующее для него значение текущей аттестации
                    {
                        Find find = wordApp.Selection.Find;
                        find.Text = item.Key;
                        find.Replacement.Text = item.Value;

                        object wrap = WdFindWrap.wdFindContinue;
                        object replace = WdReplace.wdReplaceAll;

                        find.Execute(FindText: Type.Missing,
                            MatchCase: false, MatchWholeWord: false, MatchWildcards: false,
                            MatchSoundsLike: missing, MatchAllWordForms: false, Forward: true,
                            Wrap: wrap, Format: false, ReplaceWith: missing, Replace: replace);

                    }

                    wordTable = wordDoc.Tables[2]; //Обращение к таблице результатов студентов за экзамен

                    int row = 2;
                    foreach (Criteria criteria in listCriteriaAttestation)
                    {
                        wordTable.Rows.Add(System.Reflection.Missing.Value);
                        wordTable.Cell(row, 1).Range.Text = Convert.ToString(row - 1);
                        wordTable.Cell(row, 2).Range.Text = criteria.Title;
                        wordTable.Cell(row, 3).Range.Text = "0,00";

                        foreach (StudentResult criteriaStudent in resultCurentStudent)
                        {
                            if (criteria.IdCriteria == criteriaStudent.IdCriteria)
                                wordTable.Cell(row, 3).Range.Text = criteriaStudent.NumberOfPointsForCriteria;

                        }
                        row++;
                    }

                    // имя нового файла ведомости
                    object newFile = HttpContext.Server.MapPath($"~/FilesVedomosti/Group/{attestation.Group.Title}/{attestation.Discipline.Title} {attestation.EndDate:dd.MM.yyyy}/Ведомость студента {currentStudent.LastName} {currentStudent.FirstName}.docx");

                    wordDoc.SaveAs2(newFile); //сохранить заполненный данными шаблон как новый документ
                    wordApp.ActiveDocument.Close(); //закрытие активного документа
                    wordApp?.Quit(); //отключение от приложения для работы с документами типа Word


                }
                catch (Exception ex)
                {
                    wordApp?.Quit();
                    Console.WriteLine(ex.Message);
                }

            }

            string pathZip = HttpContext.Server.MapPath($"~/FilesVedomosti/Group/{attestation.Group.Title}/{attestation.Discipline.Title} {attestation.EndDate:dd.MM.yyyy}");

            //Проверка на содержание файла ведомости по текущему экзамену
            if (System.IO.File.Exists(HttpContext.Server.MapPath($"~/FilesVedomosti/Group/{attestation.Group.Title}/{attestation.Discipline.Title} {attestation.EndDate:dd.MM.yyyy}.zip")))
            {
                System.IO.File.Delete(HttpContext.Server.MapPath($"~/FilesVedomosti/Group/{attestation.Group.Title}/{attestation.Discipline.Title} {attestation.EndDate:dd.MM.yyyy}.zip"));
            }

            ZipFile.CreateFromDirectory(pathZip, HttpContext.Server.MapPath($"~/FilesVedomosti/Group/{attestation.Group.Title}/{attestation.Discipline.Title} {attestation.EndDate:dd.MM.yyyy}.zip"));

            string pathFile = HttpContext.Server.MapPath($"~/FilesVedomosti/Group/{attestation.Group.Title}/{attestation.Discipline.Title} {attestation.EndDate:dd.MM.yyyy}.zip"); //путь до сохраненного ранее архива
            string fileType = "application/zip";
            // Имя файла - необязательно. Это то имя файла, которое будет задано скачиваемому файлу
            string file_name = $"{attestation.Discipline.Title} {attestation.EndDate:dd.MM.yyyy}.zip";

            return File(pathFile, fileType, file_name); //отправка на клиент файла ведомости

        }

        // GET: Attestations/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Attestation attestation = db.Attestation.Find(id);
            List<Student> studentlist = db.Student.Where(s => s.IdGroup == attestation.IdGroup && s.IdStatusStudent == 3).ToList();
            if (attestation == null)
            {
                return HttpNotFound();
            }
            return View(attestation);
        }

        // GET: Attestations/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Attestation attestation = db.Attestation.Find(id);


            if (attestation == null)
            {
                return HttpNotFound();
            }
            var idCurentUser = Int32.Parse(User.Identity.GetUserId());
            var curentUser = db.Teachers.Find(idCurentUser);

            var criteriasAttestaton = db.Criteria.Where(c => c.IdAttestation == attestation.IdAttestation && c.Deleted != true);

            var listOfDiscipline = db.DisciplineTeachers.Where(dis => dis.IdTeacher == idCurentUser).ToList();

            var listIdDiscipline = listOfDiscipline.Select(de => de.IdDiscipline);
            var disciplineCurrentUser = db.Discipline.Where(dcu => listIdDiscipline.Contains(dcu.IdDiscipline));

            var group = db.Group.Where(g => g.IdSpeciality == curentUser.IdSpeciality);
            var listVed = attestation.Vedomosti.ToList();
                

            AttestationCriteriasViewModel attestationCriteriasViewModel = new AttestationCriteriasViewModel
            {
                Attestations = attestation,
                Criterias = criteriasAttestaton,
                Disciplines = disciplineCurrentUser,
                Groups = group,
                countVedomisti = listVed.Count()
            };


            ViewBag.IdTypeAttestation = new SelectList(db.TypeAttestation, "IdTypeAttestation", "Title", attestation.IdTypeAttestation);
            return View(attestationCriteriasViewModel);
        }

        // POST: Attestations/Edit/5
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в разделе https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IdAttestation,IdDiscipline,StartDate,EndDate,IdTeachers,IdGroup,IdTypeAttestation")] Attestation attestation)
        {
            if (ModelState.IsValid)
            {

                db.Entry(attestation).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.IdGroup = new SelectList(db.Group, "IdGroup", "Title", attestation.IdGroup);
            ViewBag.IdDiscipline = new SelectList(db.Discipline, "IdDiscipline", "Title", attestation.IdDiscipline);
            ViewBag.IdTeachers = new SelectList(db.Teachers, "IdTeachers", "LastName", attestation.IdTeachers);
            ViewBag.IdTypeAttestation = new SelectList(db.TypeAttestation, "IdTypeAttestation", "Title", attestation.IdTypeAttestation);
            return View(attestation);
        }

        // GET: Attestations/Create
        public ActionResult Create()
        {
            // 
            var idCurentUser = Int32.Parse(User.Identity.GetUserId());
            var curentUser = db.Teachers.Find(idCurentUser);

            var listOfDiscipline = db.DisciplineTeachers.Where(dis => dis.IdTeacher == idCurentUser).ToList();
            var listIdDiscipline = listOfDiscipline.Select(de => de.IdDiscipline);
            var disciplineCurrentUser = db.Discipline.Where(dcu => listIdDiscipline.Contains(dcu.IdDiscipline));

            var group = db.Group.Where(g => g.IdSpeciality == curentUser.IdSpeciality);

            ViewBag.IdGroup = new SelectList(group, "IdGroup", "Title");
            ViewBag.IdDiscipline = new SelectList(disciplineCurrentUser, "IdDiscipline", "Title");
            ViewBag.IdCurentUser = idCurentUser;
            ViewBag.IdTypeAttestation = new SelectList(db.TypeAttestation, "IdTypeAttestation", "Title");
            Attestation attestation = new Attestation();
            return View(attestation);
        }

        [HttpPost]
        public ActionResult ImportAttestation(HttpPostedFileBase upload)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Index");
            }
            List<String> errors = new List<String>();
            if (upload != null)
            {
                // получаем имя файла
                string fileName = Path.GetFileName(upload.FileName);

                string ext = Path.GetExtension(fileName);
                if (ext == ".xlsx")
                {
                    //Проверка на содержание файла ведомости по текущему экзамену
                    if (System.IO.File.Exists(HttpContext.Server.MapPath("~/FilesImport/" + fileName)))
                    {
                        System.IO.File.Delete(HttpContext.Server.MapPath("~/FilesImport/" + fileName));
                    }
                    int countError = 0;

                    // сохраняем файл в папку FilesImport в проекте
                    upload.SaveAs(HttpContext.Server.MapPath("~/FilesImport/" + fileName));

                    Workbook wb = new Workbook(HttpContext.Server.MapPath("~/FilesImport/" + fileName));
                    WorksheetCollection collection = wb.Worksheets;

                    // Получить рабочий лист, используя его индекс
                    Worksheet worksheet = collection[0];

                    int rows = worksheet.Cells.MaxDataRow;

                    if (worksheet.Cells[1, 2].Value == null)
                    {
                        errors.Add("Неверно сформирован шаблон! Пустая чейка С2! В ячейке С2 должно находится наименование дисциплины, по которой проводится аттестация!");
                        countError++;
                    }

                    if (worksheet.Cells[2, 2].Value == null)
                    {
                        errors.Add("Неверно сформирован шаблон! Пустая ячейка С3! В ячейке С3 должна находится дата начала аттестации в формате 'дд.мм.гггг'!");
                        countError++;
                    }

                    if (worksheet.Cells[3, 2].Value == null)
                    {
                        errors.Add("Неверно сформирован шаблон! Пустая ячейка С4! В ячейке С4 должно находится дата окончания аттестации в формате 'дд.мм.гггг'!");
                        countError++;
                    }

                    if (worksheet.Cells[4, 2].Value == null)
                    {
                        errors.Add("Неверно сформирован шаблон! Пустая ячейка С5! В ячейке С5 должно находится наименование студенческой группы, у которой проводится аттестация!");
                        countError++;
                    }

                    if (worksheet.Cells[5, 2].Value == null)
                    {
                        errors.Add("Неверно сформирован шаблон! Пустая ячейка С6! В ячейке С6 должно находится наименование типа аттестации!");
                        countError++;
                    }

                    if (countError > 0)
                    {
                        return PartialView(errors);
                    }

                    string titleDiscipline = worksheet.Cells[1, 2].Value.ToString().Trim();
                    string startDate = worksheet.Cells[2, 2].Value.ToString().Trim();
                    string endDate = worksheet.Cells[3, 2].Value.ToString().Trim();
                    string titleGroup = worksheet.Cells[4, 2].Value.ToString().Trim();
                    string titleTypeAttestation = worksheet.Cells[5, 2].Value.ToString().Trim();

                    var discipline = db.Discipline.Where(d => d.Title == titleDiscipline).FirstOrDefault();
                    if (discipline == null)
                    {
                        errors.Add($"Указанная дисциплина {titleDiscipline} не найдена. Проверьте правильность написания названия дисциплины! Перечень дисциплин приведен в файле шаблона на листе 'Дисциплины'.");
                        countError++;
                    }

                    var group = db.Group.Where(g => g.Title == titleGroup).FirstOrDefault();
                    if (group == null)
                    {
                        errors.Add($"Указанная группа {titleGroup} не найдена. Проверьте правильность написания названия группы! Список групп приведен в файле шаблона на листе 'Группы'");
                        countError++;
                    }

                    var typeAttestation = db.TypeAttestation.Where(ta => ta.Title == titleTypeAttestation).FirstOrDefault();
                    if (typeAttestation == null)
                    {
                        errors.Add($"Указанная группа {titleTypeAttestation} не найдена. Проверьте правильность написания названия типа аттестации! Список типов аттестаций приведен в файле шаблона на листе 'Типы аттестаций'");
                        countError++;
                    }

                    if (!DateTime.TryParse(startDate, out DateTime succesStartDate))
                    {
                        errors.Add($"Указанная дата начала аттестации, в ячейке С3, не соответствует формату 'дд.мм.гггг'!");
                        countError++;
                    }

                    if (!DateTime.TryParse(startDate, out DateTime succesEndDate))
                    {
                        errors.Add($"Указанная дата конца аттестации, в ячейке С4, не соответствует формату 'дд.мм.гггг'!");
                        countError++;
                    }

                    if (countError > 0)
                    {
                        return PartialView(errors);
                    }

                    Attestation attestation = new Attestation();
                    attestation.IdDiscipline = discipline.IdDiscipline;
                    attestation.IdGroup = group.IdGroup;
                    attestation.IdTypeAttestation = typeAttestation.IdTypeAttestation;
                    attestation.StartDate = DateTime.Parse(startDate);
                    attestation.EndDate = DateTime.Parse(endDate);
                    attestation.IdTeachers = Int32.Parse(User.Identity.GetUserId());

                    db.Attestation.Add(attestation);
                    db.SaveChanges();

                    if (worksheet.Cells[7, 0].Value == null)
                    {
                        errors.Add("Неверно сформирован шаблон! Заголовок таблицы должны начинаться с ячейки 8A и до ячейки 8F!");
                        return PartialView(errors);
                    }

                    List<Criteria> criterias = new List<Criteria>();
                    // Цикл по строкам
                    for (int i = 8; i < rows; i++)
                    {
                        Criteria newCriteria = new Criteria();
                        newCriteria.IdAttestation = attestation.IdAttestation;
                        if (worksheet.Cells[i, 1].Value != null && worksheet.Cells[i, 1].Value.ToString() == "Итого:")
                        {
                            break;
                        }
                        if (worksheet.Cells[i, 1].Value != null)
                        {
                            newCriteria.Title = worksheet.Cells[i, 1].Value.ToString();
                        }
                        else
                        {
                            errors.Add($"Пустая ячейка B{i + 1}! Нет наименования критерия!");
                            countError++;
                        }

                        if (worksheet.Cells[i, 2].Value != null)
                        {
                            newCriteria.Description = worksheet.Cells[i, 2].Value.ToString();
                        }
                        else
                        {
                            newCriteria.Description = "";
                        }

                        if (worksheet.Cells[i, 3].Value != null)
                        {

                            if (int.TryParse(worksheet.Cells[i, 3].Value.ToString(), out int percent))
                            {
                                newCriteria.WithdrawPercent = percent.ToString();
                            }
                            else
                            {
                                errors.Add($"В ячейке D{i + 1} должно быть целое неотрицательное число!");
                                countError++;
                            }
                        }
                        else
                        {
                            newCriteria.WithdrawPercent = "0";
                        }

                        if (worksheet.Cells[i, 4].Value != null)
                        {

                            if (decimal.TryParse(worksheet.Cells[i, 4].Value.ToString().Trim(), out decimal remPoint))
                            {
                                newCriteria.RemoveAPoint = remPoint.ToString();
                            }
                            else
                            {
                                errors.Add($"В ячейке E{i + 1} должно быть число формата 0.00!");
                                countError++;
                            }
                        }
                        else
                        {
                            newCriteria.RemoveAPoint = "0";
                        }

                        if (worksheet.Cells[i, 5].Value != null)
                        {

                            if (decimal.TryParse(worksheet.Cells[i, 5].Value.ToString().Trim(), out decimal point))
                            {
                                newCriteria.NumberOfPionts = point.ToString();
                            }
                            else
                            {
                                errors.Add($"В ячейке F{i + 1} должно быть число формата 0.00!");
                                countError++;
                            }
                        }
                        else
                        {
                            errors.Add($"Пустая ячейка F{i + 1}! Нет максимального балла за критерий!");
                            countError++;
                        }


                        if (countError > 0)
                        {
                            countError = 0;
                            continue;
                        }

                        criterias.Add(newCriteria);

                    }
                    if (errors.Count > 0)
                    {
                        return PartialView(errors);
                    }
                    db.Criteria.AddRange(criterias);
                    db.SaveChanges();
                    System.IO.File.Delete(HttpContext.Server.MapPath("~/FilesImport/" + fileName));
                    errors.Add("Импорт аттестации с криетриями успешно завершен. Для отображения данных необходимо закрыть данное окно, после чего нажать клавишу 'F5'.");
                }
                else
                {
                    errors.Add("Загружен неверный формат документа. Должен быть формат .xlsx, а не " + ext);
                    return PartialView(errors);
                }

            }
            else
            {
                errors.Add("Не был выбран файл для импорта!");
                return PartialView(errors);
            }
            return PartialView(errors);
        }


        [HttpPost]
        public ActionResult ImportCriterias(int IdAttestation, HttpPostedFileBase upload)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Index");
            }
            List<String> errors = new List<String>();
            if (upload != null)
            {
                // получаем имя файла
                string fileName = Path.GetFileName(upload.FileName);
                string ext = Path.GetExtension(fileName);
                if (ext == ".xlsx")
                {
                    //Проверка на содержание файла ведомости по текущему экзамену
                    if (System.IO.File.Exists(HttpContext.Server.MapPath("~/FilesImport/" + fileName)))
                    {
                        System.IO.File.Delete(HttpContext.Server.MapPath("~/FilesImport/" + fileName));
                    }
                    int countError = 0;

                    // сохраняем файл в папку FilesImport в проекте
                    upload.SaveAs(HttpContext.Server.MapPath("~/FilesImport/" + fileName));

                    Workbook wb = new Workbook(HttpContext.Server.MapPath("~/FilesImport/" + fileName));
                    WorksheetCollection collection = wb.Worksheets;

                    // Получить рабочий лист, используя его индекс
                    Worksheet worksheet = collection[0];

                    int rows = worksheet.Cells.MaxDataRow;

                    if (worksheet.Cells[7, 0].Value == null)
                    {
                        errors.Add("Неверно сформирован шаблон! Заголовок таблицы должны начинаться с ячейки 8A и до ячейки 8F!");
                        return PartialView(errors);
                    }

                    List<Criteria> criterias = new List<Criteria>();
                    // Цикл по строкам
                    for (int i = 8; i < rows; i++)
                    {
                        Criteria newCriteria = new Criteria();
                        newCriteria.IdAttestation = IdAttestation;
                        if (worksheet.Cells[i, 1].Value != null && worksheet.Cells[i, 1].Value.ToString() == "Итого:")
                        {
                            break;
                        }
                        if (worksheet.Cells[i, 1].Value != null)
                        {
                            newCriteria.Title = worksheet.Cells[i, 1].Value.ToString();
                        }
                        else
                        {
                            errors.Add($"Пустая ячейка B{i + 1}! Нет наименования критерия!");
                            countError++;
                        }

                        if (worksheet.Cells[i, 2].Value != null)
                        {
                            newCriteria.Description = worksheet.Cells[i, 2].Value.ToString();
                        }
                        else
                        {
                            newCriteria.Description = "";
                        }

                        if (worksheet.Cells[i, 3].Value != null)
                        {

                            if (int.TryParse(worksheet.Cells[i, 3].Value.ToString(), out int percent))
                            {
                                newCriteria.WithdrawPercent = percent.ToString();
                            }
                            else
                            {
                                errors.Add($"В ячейке D{i + 1} должно быть целое неотрицательное число!");
                                countError++;
                            }
                        }
                        else
                        {
                            newCriteria.WithdrawPercent = "0";
                        }

                        if (worksheet.Cells[i, 4].Value != null)
                        {

                            if (decimal.TryParse(worksheet.Cells[i, 4].Value.ToString().Trim(), out decimal remPoint))
                            {
                                newCriteria.RemoveAPoint = remPoint.ToString();
                            }
                            else
                            {
                                errors.Add($"В ячейке E{i + 1} должно быть число формата 0.00!");
                                countError++;
                            }
                        }
                        else
                        {
                            newCriteria.RemoveAPoint = "0";
                        }

                        if (worksheet.Cells[i, 5].Value != null)
                        {

                            if (decimal.TryParse(worksheet.Cells[i, 5].Value.ToString().Trim(), out decimal point))
                            {
                                newCriteria.NumberOfPionts = point.ToString();
                            }
                            else
                            {
                                errors.Add($"В ячейке F{i + 1} должно быть число формата 0.00!");
                                countError++;
                            }
                        }
                        else
                        {
                            errors.Add($"Пустая ячейка F{i + 1}! Нет максимального балла за критерий!");
                            countError++;
                        }

                        if (countError > 0)
                        {
                            countError = 0;
                            continue;
                        }

                        criterias.Add(newCriteria);

                    }
                    if (errors.Count > 0)
                    {
                        return PartialView(errors);
                    }
                    db.Criteria.AddRange(criterias);
                    db.SaveChanges();
                    System.IO.File.Delete(HttpContext.Server.MapPath("~/FilesImport/" + fileName));
                    errors.Add("Импорт критериев успешно завершен. Для отображения данных необходимо закрыть данное окно, после чего нажать клавишу 'F5'.");
                }
                else
                {
                    errors.Add("Загружен неверный формат документа. Должен быть формат .xlsx, а не " + ext);
                    return PartialView(errors);
                }

            }
            else
            {
                errors.Add("Не был выбран файл для импорта!");
                return PartialView(errors);
            }
            return PartialView(errors);
        }

        // POST: Attestations/Create
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в разделе https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IdAttestation,IdDiscipline,StartDate,EndDate,IdTeachers,IdGroup,IdTypeAttestation")] Attestation attestation)
        {
            if (ModelState.IsValid)
            {
                db.Attestation.Add(attestation);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            var idCurentUser = Int32.Parse(User.Identity.GetUserId());
            var curentUser = db.Teachers.Find(idCurentUser);

            var listOfDiscipline = db.DisciplineTeachers.Where(dis => dis.IdTeacher == idCurentUser).ToList();
            var listIdDiscipline = listOfDiscipline.Select(de => de.IdDiscipline);
            var disciplineCurrentUser = db.Discipline.Where(dcu => listIdDiscipline.Contains(dcu.IdDiscipline));

            var group = db.Group.Where(g => g.IdSpeciality == curentUser.IdSpeciality);

            ViewBag.IdGroup = new SelectList(group, "IdGroup", "Title");
            ViewBag.IdDiscipline = new SelectList(disciplineCurrentUser, "IdDiscipline", "Title");
            ViewBag.IdTeachers = idCurentUser;
            ViewBag.IdTypeAttestation = new SelectList(db.TypeAttestation, "IdTypeAttestation", "Title");
            return View(attestation);
        }

        // GET: Attestations/Delete/5
        public ActionResult Delete(int? id) // Запрос на удаление аттестации, открывает страницу с удалением аттестации
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Attestation attestation = db.Attestation.Find(id);
            if (attestation == null)
            {
                return HttpNotFound();
            }
            return View(attestation);
        }

        // POST: Attestations/Delete/5
        [HttpPost, ActionName("Delete")] // Запрос на подтверждение удаления выбранной аттестации
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Attestation attestation = db.Attestation.Find(id);
            attestation.Deleted = true;
            db.Entry(attestation).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult DeleteAllCriteria(int? idAttestation)
        {
            var listCriteria = db.Criteria.Where(c => c.IdAttestation == idAttestation && c.Deleted != true);

            foreach (Criteria crt in listCriteria)
            {
                crt.Deleted = true;
                db.Entry(crt).State = EntityState.Modified;
            }
            
            db.SaveChanges();
            return RedirectToAction("Edit", "Attestations", new { id = idAttestation });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
