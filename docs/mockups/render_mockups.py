from pathlib import Path

from PIL import Image, ImageDraw, ImageFont


ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / "images"
OUT.mkdir(parents=True, exist_ok=True)

FONT = "C:/Windows/Fonts/arial.ttf"
BOLD = "C:/Windows/Fonts/arialbd.ttf"

font = ImageFont.truetype(FONT, 18)
font_b = ImageFont.truetype(BOLD, 18)
font_sm = ImageFont.truetype(FONT, 14)
font_sm_b = ImageFont.truetype(BOLD, 14)
font_title = ImageFont.truetype(BOLD, 28)


def rect(draw, xy, fill, outline=None, width=1):
    draw.rectangle(xy, fill=fill, outline=outline, width=width)


def text(draw, xy, value, f=font, fill=(30, 30, 30), anchor=None):
    draw.text(xy, value, font=f, fill=fill, anchor=anchor)


def navbar(draw, width, title="Расписание | АИСТбд-21"):
    rect(draw, (0, 0, width, 52), (248, 248, 248), (220, 220, 220))
    text(draw, (28, 16), title, font_b)
    x = 280
    for item in [
        "Представиться",
        "Баг-трекер",
        "Журнал",
        "Расписание кабинета",
        "Посещаемость",
        "Ведомость",
    ]:
        text(draw, (x, 17), item, font_sm, (70, 70, 70))
        x += int(font_sm.getlength(item)) + 30


def panel(draw, x, y, width, title, subtitle=None):
    rect(draw, (x, y, x + width, y + 82), (255, 255, 255), (188, 232, 241))
    rect(draw, (x, y, x + width, y + 42), (217, 237, 247), (188, 232, 241))
    text(draw, (x + width // 2, y + 13), title, font_b, (49, 112, 143), "ma")
    if subtitle:
        text(draw, (x + width // 2, y + 54), subtitle, font, (49, 112, 143), "ma")


def render_schedule_mock():
    width, height = 1280, 900
    image = Image.new("RGB", (width, height), "white")
    draw = ImageDraw.Draw(image)
    navbar(draw, width)
    panel(draw, 70, 82, 1140, "Вторник, 01.02.2022", "АИСТбд-21")

    x0, y0 = 90, 195
    col_widths = [130, 360, 250, 300]
    headers = ["время", "дисциплина", "ауд.", "Преподаватель"]

    x = x0
    for col_width, header in zip(col_widths, headers):
        rect(draw, (x, y0, x + col_width, y0 + 42), (245, 245, 245), (40, 40, 40), 2)
        text(draw, (x + col_width / 2, y0 + 13), header, font_b, anchor="ma")
        x += col_width

    rows = [
        ("8:30\n9:50", "Правоведение  лек.", "Discord", "Ерохина Е.А.", "ok"),
        ("10:00\n11:20", "Философия  лек.", "Discord", "Зиновьева Э.Н.", "ok"),
        ("11:50\n13:10", "Организация ЭВМ и систем  лек.", "413", "Попов Н.А.", "warn"),
        ("13:20\n14:40", "Надежность инф.сист.  лек.", "413", "Попов Н.А.", "bad"),
        ("14:45\n16:05", "Физ. культура и спорт  лек.", "Discord", "Черненькая Е.В.", "ok"),
    ]

    y = y0 + 42
    for time_value, discipline, cabinet, teacher, state in rows:
        fill = (252, 248, 227) if state == "warn" else (242, 222, 222) if state == "bad" else (223, 240, 216)
        x = x0
        for index, (col_width, value) in enumerate(zip(col_widths, [time_value, discipline, cabinet, teacher])):
            rect(draw, (x, y, x + col_width, y + 78), fill, (40, 40, 40), 1)
            lines = value.split("\n")
            yy = y + 16 if len(lines) == 1 else y + 8
            for line in lines:
                text(draw, (x + col_width / 2, yy), line, font_sm_b if index == 0 else font_sm, anchor="ma")
                yy += 24
            x += col_width
        if state == "bad":
            text(draw, (x0 + col_widths[0] + col_widths[1] - 30, y + 24), "X", font_title, (170, 0, 0))
        y += 78

    text(
        draw,
        (90, 820),
        "Реконструкция внешнего вида legacy PHP-интерфейса: Bootstrap-таблица, подсветка проблемной пары и ссылка на подробности.",
        font_sm,
        (90, 90, 90),
    )
    image.save(OUT / "web-schedule-mock.png")


def render_conflict_mock():
    width, height = 1280, 900
    image = Image.new("RGB", (width, height), "white")
    draw = ImageDraw.Draw(image)
    navbar(draw, width, "Расписание | ИАТУ")
    panel(draw, 70, 82, 1140, "Сейчас показана 3 неделя.", "С 31.01.2022 по 06.02.2022")
    text(draw, (90, 188), "← Предыдущая", font, (60, 118, 161))
    text(draw, (1060, 188), "Следующая →", font, (60, 118, 161))

    y = 235
    alerts = [
        "Преподаватель (Попов Н.А.) обнаружен в нескольких кабинетах одновременно!",
        "В одном кабинете (413) обнаружено несколько преподавателей одновременно!",
    ]

    for alert in alerts:
        rect(draw, (90, y, 1190, y + 52), (242, 222, 222), (235, 204, 209))
        text(draw, (112, y + 16), alert, font_b, (132, 53, 52))
        y += 70

        col_widths = [120, 100, 150, 380, 80, 250, 100, 140]
        headers = ["дата", "время", "группа", "дисциплина", "тип", "преподаватель", "кабинет", "файл"]
        x = 90
        for col_width, header in zip(col_widths, headers):
            rect(draw, (x, y, x + col_width, y + 36), (245, 245, 245), (210, 210, 210))
            text(draw, (x + 8, y + 10), header, font_sm_b)
            x += col_width

        data = [
            ["01.02.2022", "10:00", "АИСТбд-21", "Организация ЭВМ и систем", "лек.", "Попов Н.А.", "413", "week3.xls"],
            ["01.02.2022", "10:00", "АИСТбд-31", "Цифр.выч.устр.и микр.сис.", "лек.", "Попов Н.А.", "304", "week3.xls"],
        ]
        y += 36
        for row in data:
            x = 90
            for col_width, value in zip(col_widths, row):
                rect(draw, (x, y, x + col_width, y + 34), (255, 255, 255), (225, 225, 225))
                text(draw, (x + 8, y + 9), value, font_sm)
                x += col_width
            y += 34
        y += 34

    text(
        draw,
        (90, 815),
        "Реконструкция страницы problem_finder.php: недельная проверка конфликтов с подробными строками расписания.",
        font_sm,
        (90, 90, 90),
    )
    image.save(OUT / "web-conflict-tracker-mock.png")


def render_statement_mock():
    width, height = 1600, 900
    image = Image.new("RGB", (width, height), "white")
    draw = ImageDraw.Draw(image)
    navbar(draw, width, "Расписание | ИАТУ")

    rect(draw, (580, 92, 1020, 142), (255, 255, 255), (204, 204, 204))
    text(draw, (800, 108), "Попов Н.А.  v", font_b, (60, 60, 60), "ma")

    text(draw, (90, 180), "Ведомость занятий за семестр", font_title, (40, 40, 40))
    text(draw, (90, 218), "Реконструкция страницы statement.php: выбор преподавателя и сводка занятий по типам часов.", font_sm, (90, 90, 90))

    x0, y0 = 70, 270
    col_widths = [48, 100, 135, 260, 120, 210, 80, 145, 120, 120, 100]
    headers = [
        "#",
        "Дата",
        "Время",
        "Дисциплина",
        "Группа",
        "Содержание занятий",
        "Лекция",
        "семинар и практ.",
        "Консультации",
        "Курсовая",
        "Экзамены",
    ]

    x = x0
    for col_width, header in zip(col_widths, headers):
        rect(draw, (x, y0, x + col_width, y0 + 44), (245, 245, 245), (190, 190, 190))
        text(draw, (x + 6, y0 + 14), header, font_sm_b)
        x += col_width

    rows = [
        ["1", "01.02.2022", "10:00 до 11:20", "Организация ЭВМ и систем", "АИСТбд-21", "лек.", "2", "0", "0", "0", "0"],
        ["2", "03.02.2022", "11:50 до 13:10", "Моделирование инф.систем", "АИСТбд-31", "лаб.", "0", "2", "0", "0", "0"],
        ["3", "08.02.2022", "13:20 до 14:40", "Распределенные информ. системы", "АИСТбд-41", "пр.", "0", "2", "0", "0", "0"],
        ["4", "15.02.2022", "10:00 до 11:20", "Организация ЭВМ и систем", "АИСТбд-21", "лек.", "2", "0", "0", "0", "0"],
        ["5", "22.02.2022", "14:45 до 16:05", "Цифр.выч.устр.и микр.сис.", "АИСТбд-31", "лек.", "2", "0", "0", "0", "0"],
    ]

    y = y0 + 44
    for row in rows:
        x = x0
        for col_width, value in zip(col_widths, row):
            rect(draw, (x, y, x + col_width, y + 42), (223, 240, 216), (210, 210, 210))
            text(draw, (x + 6, y + 12), value, font_sm)
            x += col_width
        y += 42

    rect(draw, (x0, y + 24, x0 + 1450, y + 72), (217, 237, 247), (188, 232, 241))
    text(draw, (x0 + 18, y + 39), "Сценарий старост: быстро получить основу ведомости по преподавателю вместо ручного подсчета занятий по Excel.", font_b, (49, 112, 143))
    image.save(OUT / "web-statement-mock.png")


if __name__ == "__main__":
    render_schedule_mock()
    render_conflict_mock()
    render_statement_mock()
