
# https://github.com/mindee/doctr

from doctr.io import DocumentFile
from doctr.models import ocr_predictor

image_doc = DocumentFile.from_images("bin/Debug/net5.0-windows/test.jpg")

model = ocr_predictor(pretrained=True)

result = model(image_doc)
#result.show(image_doc)

print(result.export())