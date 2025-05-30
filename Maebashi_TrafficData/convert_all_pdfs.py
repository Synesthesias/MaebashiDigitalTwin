import subprocess
import os
from glob import glob

src_dir = 'Maebashi_TrafficData'
for pdf_path in glob(os.path.join(src_dir, '**', '*.pdf'), recursive=True):
    txt_path = os.path.splitext(pdf_path)[0] + '.txt'
    result = subprocess.run(['pdftotext', '-layout', pdf_path, txt_path], capture_output=True, text=True)
    if result.returncode == 0:
        print(f'変換完了: {pdf_path} → {txt_path}')
    else:
        print(f'変換失敗: {pdf_path}\n{result.stderr}') 