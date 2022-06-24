using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using NPOI.XWPF.UserModel;
using NPOI.OpenXmlFormats.Wordprocessing;
using CarWashApp.Entities;

namespace CarWashApp.Classes
{
    /// <summary>
    /// Класс, позволяющий создать чек с данными о заказе.
    /// Документ-чек создается на основе шаблона, представленного в папке "Resources".
    /// </summary>
    public class DocumentBuilder
    {
        #region Область: Константы, содержащее имена областей для замены.

        /// <summary>
        /// Константа, содержащая название закладки с адресом доставки заказа.
        /// </summary>
        public const String OrderDeliveryAddress = "Address";

        /// <summary>
        /// Константа, содержащая название закладки с именем сотрудника, оформившего заказ.
        /// </summary>
        public const String OrderEmployeeName = "AdminName";

        /// <summary>
        /// Константа, содержащая название закладки с именем клиента текущего заказа.
        /// </summary>
        public const String OrderClientName = "ClientName";

        /// <summary>
        /// Константа, содержащая название закладки с количеством денег на сдачу.
        /// </summary>
        public const String FullOrderCashBack = "CashBack";

        /// <summary>
        /// Константа, содержащая название закладки с полной стоимостью заказа (без учета НДС).
        /// </summary>
        public const String FullOrderPrice = "FinalPrice";

        /// <summary>
        /// Константа, содержащая название закладки с ID заказа в БД (в документе подписано как № чека).
        /// </summary>
        public const String OrderId = "ID";

        /// <summary>
        /// Константа, содержащая название закладки с количеством денег, которые необходимо оплатить по заказу.
        /// </summary>
        public const String MoneyCount = "MoneyCount";

        /// <summary>
        /// Константа, содержащая название закладки с датой начала заказа.
        /// </summary>
        public const String OrderBeginDate = "OrderDate";

        /// <summary>
        /// Константа, содержащая название закладки со списком названий услуг в заказе.
        /// </summary>
        public const String AllServicesTitlesList = "ServicesNames";

        /// <summary>
        /// Константа, содержащая название закладки со списком цен услуг в заказе.
        /// </summary>
        public const String AllServicesPricesList = "ServicesPrices";

        /// <summary>
        /// Константа, содержащая название закладки с величиной (абсолютной) НДС данного заказа.
        /// </summary>
        public const String OrderTax = "TaxesPrice";

        /// <summary>
        /// Константа, содержащая название закладки с количеством денег, полученных от клиента.
        /// </summary>
        public const String TotalBillByClient = "TotalReceived";
        #endregion

        #region Область: Свойства.

        /// <summary>
        /// Свойство, содержащее путь, по которому будет создан чек с данными о заказе. <br />
        /// Если по указанному пути будет присутствовать другой файл с таким же названием, то он будет перезаписан.
        /// </summary>
        public String Path { get; private set; }

        /// <summary>
        /// Свойство, содержащее ссылку на текущий заказ.
        /// </summary>
        public Order Order { get; private set; }

        /// <summary>
        /// Свойство, содержащее все данные о документе с чеком.
        /// <br /> <br />
        /// Изначально документ создается на основе шаблона. <br />
        /// Затем эти данные изменяются и этот измененный документ сохраняется в другом месте.
        /// </summary>
        public XWPFDocument Document { get; private set; }
        #endregion

        #region Область: Конструктор.

        /// <summary>
        /// Конструктор класса.
        /// <br /> <br />
        /// Кроме инициализации свойств, также запускает поток считывания документа шаблона для формирования чека.
        /// </summary>
        /// <param name="path">Путь, по которому будет создан чек.</param>
        /// <param name="order">Текущий заказ.</param>
        public DocumentBuilder(String path, Order order)
        {
            Path = path;
            Order = order;

            // Запускаем поток на считывание шаблона.
            using (Stream stream = new FileStream(GetTemplateDocumentPath(), FileMode.OpenOrCreate))
            {
                Document = new XWPFDocument(stream);
            }
        }
        #endregion

        #region Область: Функции.

        /// <summary>
        /// Чтобы получить документ-шаблон, выделена эта функция.
        /// </summary>
        /// <returns>Путь к документу-шаблону.</returns>
        private String GetTemplateDocumentPath()
        {
            String currentDirectory = Environment.CurrentDirectory;
            currentDirectory = Directory.GetParent(currentDirectory).Parent.FullName;

            // Так как в классе уже определено свойство "Path", то для вызова класса придется определить его полностью.
            String pathToTemplate = System.IO.Path.Combine(currentDirectory, "Resources", "!Template.docx");

            return pathToTemplate;
        }

        /// <summary>
        /// Функция, запускающая весь алгоритм создания нового чека.
        /// </summary>
        public void CreateNewDocument()
        {
            // Для начала необходимо создать связанный список.
            List<(XWPFParagraph, CT_Bookmark)> bookmarks = GetBookmarksWithParagraphInTemplate();

            FormatBookmarkPlaceholders(bookmarks);

            SaveNewDocument();
        }

        /// <summary>
        /// Формирует связанный список из кортежей со связкой "Параграф" — "Закладка в документе".
        /// <br /> <br />
        /// Чтобы в NPOI изменить текст в закладке, нужно получить его параграф. <br />
        /// Чтобы сформировать стабильные связи мы создаем кортежи из двух значений.
        /// </summary>
        /// <returns>Список.</returns>
        private List<(XWPFParagraph, CT_Bookmark)> GetBookmarksWithParagraphInTemplate()
        {
            // Для начала просто создаем список.
            List<(XWPFParagraph, CT_Bookmark)> marksWithParagraph = new List<(XWPFParagraph, CT_Bookmark)>(1);

            // Так как в документе преобладают таблицы, нужно обработать их. Для этого получаем список всех таблиц и начинаем итерирование через цикл "foreach".
            foreach (XWPFTable table in Document.Tables)
            {
                // Получив определенную таблицу начинаем обрабатывать уже её. В таблицах есть строки. Получаем список строк и пробегаемся по ним через цикл.
                foreach (XWPFTableRow row in table.Rows)
                {
                    // Получив конкретную строку мы можем спуститься ещё дальше, до ячеек. Получаем список ячеек текущей строки и начинаем проверять её.
                    // В отличие от предыдущих случаев, здесь геттер не был вынесен в свойство, поэтому как в Java, мы просто вызываем специальную функцию ("GetTableRows()").
                    foreach (XWPFTableCell cell in row.GetTableCells())
                    {
                        // Получив конкретную ячейку наконец переходим к тексту. Текст состоит из параграфов. Получаем список параграфов и пробегаемся по нему.
                        foreach (XWPFParagraph paragraph in cell.Paragraphs)
                        {
                            // И вот, уже в параграфе мы можем получить его системный элемент "CTP". Он содержит различную информацию об определенном участке текста.
                            // Получив "CTP" мы вызываем его функцию, чтобы получить список всех закладок, находящихся в области, относящейся к этому "CTP" (т.е. в этом параграфе).
                            // Но по умолчанию эта функция возвращает не список, а "IEnumerable<CT_Bookmark>", так что его ещё нужно преобразовать в оригинальный список ("List<T>").
                            foreach (CT_Bookmark bookmark in paragraph.GetCTP().GetBookmarkStartList().ToList())
                            {
                                // И вот, мы добавляем соответствующие элементы в наш список.
                                marksWithParagraph.Add((paragraph, bookmark));
                            }
                        }
                    }
                }
            }

            return marksWithParagraph;
        }

        /// <summary>
        /// Форматирует (изменяет значения плейсхолдеров) в закладках шаблона, значениями текущего заказа.
        /// </summary>
        /// <param name="bookmarks">Список со связанными значениями и данными закладок.</param>
        private void FormatBookmarkPlaceholders(List<(XWPFParagraph, CT_Bookmark)> bookmarks)
        {
            // Для начала необходимо рассчитать кое-какие данные о текущем заказе: цену с НДС, количество полученных денег (да, эта переменная рассчитывается через рандом) и сдачу.
            Decimal priceWithTax = Order.FinalPrice_Bind + Order.TaxFromPrice;
            Decimal moneyCount = new Random().Next(Convert.ToInt32(priceWithTax), Convert.ToInt32(priceWithTax) + 2000);
            Decimal change = moneyCount - priceWithTax;

            // И вот, мы начинаем уже пробегаться по самим закладкам, чтобы изменять шаблон.
            foreach ((XWPFParagraph parent, CT_Bookmark mark) bookmark in bookmarks)
            {
                // Значений много, поэтому для чистоты кода все было вынесено в блок "switch...cases".
                switch (bookmark.mark.name)
                {
                    // Если смысл какого-либо участка кода будет непонятен, то стоит посмотреть описание константы, отвечающей за него. Ответ появится через полсекунды.

                    case OrderEmployeeName:
                        bookmark.parent.ReplaceText("{Имя}", Order.Employe.FullName_Bind);
                        break;

                    case OrderClientName:
                        bookmark.parent.ReplaceText("{Пользователь}", Order.Client.ClientFullNameWithInitials);
                        break;

                    case OrderDeliveryAddress:
                        bookmark.parent.ReplaceText("{Адрес}", Order.Address);
                        break;

                    case OrderId:
                        bookmark.parent.ReplaceText("{ID_В_Системе}", Order.ID.ToString());
                        break;

                    case AllServicesTitlesList:
                        FillServiceTitlesList(bookmark.parent);
                        break;

                    case AllServicesPricesList:
                        FillServicePricesList(bookmark.parent);
                        break;

                    case OrderBeginDate:
                        bookmark.parent.ReplaceText("00.00.00 00:00", Order.OrderDateTime.ToString("dd.MM.yy HH:mm"));
                        break;

                    case FullOrderPrice:
                        bookmark.parent.ReplaceText("=000.00", Order.FinalPrice_Bind.ToString("0.00"));
                        break;

                    case OrderTax:
                        bookmark.parent.ReplaceText("{Налог}", Order.TaxFromPrice.ToString("0.00"));
                        break;

                    case MoneyCount:
                        bookmark.parent.ReplaceText("{Счет}", priceWithTax.ToString("0.00"));
                        break;

                    case TotalBillByClient:
                        bookmark.parent.ReplaceText("{Немного_Больше}", moneyCount.ToString("0.00"));
                        break;

                    case FullOrderCashBack:
                        bookmark.parent.ReplaceText("{Простые_Расчеты}", change.ToString("0.00"));
                        break;

                    // Блок "default" не был добавлен, на случай возможной модификации документа-шаблона.
                }
            }
        }

        /// <summary>
        /// Заполнение данных о названиях услуг в заказе, содержит очень много кода, поэтому было вынесено в отдельную функцию.
        /// </summary>
        /// <param name="paragraph">Параграф, в котором находится закладка и в котором будут размещены данные об услугах заказа.</param>
        private void FillServiceTitlesList(XWPFParagraph paragraph)
        {
            // Для начала создаем объект "Run". Условно, это некоторая выделенная область.
            XWPFRun run = paragraph.CreateRun(); // Создавая "Run" таким образом мы сразу же добавляем его в текущий параграф и получаем ссылку на него в памяти.
            run.SetFontFamily("Georgia", FontCharRange.Ascii); // Задаем этой области шрифт. В данном случае "Georgia" с диапазоном символов "Ascii".
            run.FontSize = 14; // Устанавливаем размер шрифта.
            run.AddBreak(BreakType.TEXTWRAPPING); // Ну и добавляем разрыв, чтобы последующий текст не "клеился" к заголовку.

            // Используем цикл "for", чтобы пробежаться по всем элементам в списке услуг текущего заказа.
            for (int i = 0; i < Order.Service.Count; i++)
            {
                Service service = Order.Service.ToList()[i]; // Получаем текущий элемент в списке услуг.
                run.AppendText($"{i + 1}. {service.Title}"); // Цикл "for" (вместо "foreach") использовался, чтобы можно было легко проставить номера услуг в заказе.

                // Чтобы не добавлять разрыв после последнего элемента в списке услуг, делаем базовую проверку на текущий индекс.
                if (i != Order.Service.Count - 1) 
                    run.AddBreak(BreakType.TEXTWRAPPING); // Функция "AddBreak()" добавляет разрыв в текст. Отправляя туда параметр "BreakType.TEXTWRAPPING" мы задаем разрыв на всю строку (аналог "<br />" из HTML).
            }
        }

        /// <summary>
        /// Заполнение данных о ценах услуг в заказе, содержит очень много кода, поэтому было вынесено в отдельную функцию.
        /// </summary>
        /// <param name="paragraph">Параграф, в котором находится закладка и в котором будут размещены данные об услугах заказа.</param>
        private void FillServicePricesList(XWPFParagraph paragraph)
        {
            // Для начала создаем объект "Run". Условно, это некоторая выделенная область.
            XWPFRun run = paragraph.CreateRun(); // Создавая "Run" таким образом мы сразу же добавляем его в текущий параграф и получаем ссылку на него в памяти.
            run.SetFontFamily("Georgia", FontCharRange.Ascii); // Задаем этой области шрифт. В данном случае "Georgia" с диапазоном символов "Ascii".
            run.FontSize = 14; // Устанавливаем размер шрифта.
            run.AddBreak(BreakType.TEXTWRAPPING); // Ну и добавляем разрыв, чтобы последующий текст не "клеился" к заголовку.

            // Используем цикл "for", чтобы пробежаться по всем элементам в списке услуг текущего заказа.
            for (int i = 0; i < Order.Service.Count; i++)
            {
                Service service = Order.Service.ToList()[i]; // Получаем текущий элемент в списке услуг.
                run.AppendText($"{service.Price:0.00}"); // Используем продвинутую 

                // Цикл "for" (вместо "foreach") использовался для этой проверки.
                if (i != Order.Service.Count - 1)
                    run.AddBreak(BreakType.TEXTWRAPPING); // Функция "AddBreak()" добавляет разрыв в текст. Отправляя туда параметр "BreakType.TEXTWRAPPING" мы задаем разрыв на всю строку (аналог "<br />" из HTML).
            }
        }

        /// <summary>
        /// Выполняет сохранение документа в новом расположении после его полной генерации.
        /// </summary>
        private void SaveNewDocument()
        {
            // Создаем новый поток данных (указывая более абстрактный тип и отправляя туда более определенный тип мы выполняем процедуру т.н. "UpCast", ...
            // ... когда более специфичный тип приводится к более общему, сохраняя все значения общих элементов, но скрывая более низкоуровневые).
            using (Stream stream = new FileStream(Path, FileMode.Create, FileAccess.ReadWrite))
            {
                Document.Write(stream); // Сохраняем документ.
            }
        }
        #endregion
    }
}
