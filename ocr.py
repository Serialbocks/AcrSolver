
# https://github.com/mindee/doctr


import json
import sys
import os

from doctr.io import DocumentFile
from doctr.models import ocr_predictor

def print_stdout(text):
    print(text)
    sys.stdout.flush()

model = ocr_predictor(pretrained=True)

print_stdout('Ready')
for lineAll in sys.stdin:
    line = lineAll.rstrip()
    if 'EXIT' == line.upper():
        break

    if not os.path.isfile(line):
        print_stdout('File not found')
        continue
    image_doc = DocumentFile.from_images(line)

    print_stdout('processing')
    try:
        result = model(image_doc)
    except:
        print_stdout('Done')
        continue
    print_stdout('Writing')
    #result.show(image_doc)
    with open('ocr.json', 'w') as f:
        f.write(json.dumps(result.export()))
    print_stdout('Done')
