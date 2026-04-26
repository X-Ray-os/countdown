import re,datetime

def parse_span(raw):
    formatted = re.sub(r"\s+", " ", raw).strip().lower()
    match = re.match(r"(\d+)(\w+)", formatted)
    if not match:
        raise ValueError(f"invalid span: {raw}")
    quantity, unit = match.groups()
    return datetime.timedelta(**{unit: int(quantity)})

class CountDown:
    def __init__(self,name,span:datetime.timedelta):
        self.name = name
        self.span = span

        self.status = False
        self.lastStartTime = datetime.datetime.now()
        self.lastPauseRemain = span

    def getRemain(self):
        if not self.status:
            return self.lastPauseRemain
        elapsed = datetime.datetime.now() - self.lastStartTime
        return self.lastPauseRemain - elapsed
    
    def start(self):
        if self.status == False:
            self.status = True
            self.lastStartTime = datetime.datetime.now()

    def pause(self):
        if self.status:
            self.lastPauseRemain = self.getRemain()
            self.status = False

    def reset(self):
        self.status = False
        self.lastPauseRemain = self.span

    def percentage(self):
        span = self.span.total_seconds()
        remainlen = self.getRemain().total_seconds()
        return round(1-remainlen/span,3)
    
def cd_holder(cd:CountDown):
    while True:
        remain = cd.getRemain()
        if remain <= 0:
            cd.status = False
        yield cd.status, remain