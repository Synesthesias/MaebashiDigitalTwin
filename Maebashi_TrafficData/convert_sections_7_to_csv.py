import os
import re
import csv
import shutil

try:
    print('Start convert_sections_7_to_csv.py', flush=True)
    # 出力先フォルダを初期化
    csv_dir = 'Maebashi_TrafficData/csvs_py_sections_7'
    if os.path.exists(csv_dir):
        shutil.rmtree(csv_dir)
    os.makedirs(csv_dir, exist_ok=True)

    # 既存CSVからレーン名→リンクIDのマッピングを作成
    lane_to_linkid = {}
    with open('Maebashi_TrafficData/lane_to_linkid.csv', encoding='utf-8') as f:
        reader = csv.DictReader(f)
        for row in reader:
            lane_to_linkid[row['lanename']] = row['linkid']

    # 解析対象ファイル
    input_files = [
        'Maebashi_TrafficData/1月9日_7_A.txt',
        'Maebashi_TrafficData/1月9日_7_B.txt',
        'Maebashi_TrafficData/1月9日_7_C.txt',
        'Maebashi_TrafficData/1月16日_7_A.txt',
        'Maebashi_TrafficData/1月16日_7_B.txt',
        'Maebashi_TrafficData/1月16日_7_C.txt',
    ]

    header = ['集計開始時刻', '集計終了時刻', 'リンクID', '交通量', 'レーン名']

    for file in input_files:
        print(f'Processing: {file}', flush=True)
        with open(file, encoding='utf-8') as f:
            lines = [line.strip() for line in f if line.strip()]
        # データ行の開始位置を自動検出（「7時台」で始まる行を探す）
        data_start = None
        for idx, line in enumerate(lines):
            if re.match(r'7時台', line):
                data_start = idx
                break
        if data_start is None:
            print('データ開始行が見つかりません', flush=True)
            continue
        data_lines = lines[data_start:data_start+13]
        in_values = []
        out_values = []
        for dl in data_lines:
            dl = dl.replace('\t', ' ')
            dl = re.sub(r' +', ' ', dl)
            # 2つの「時台」で分割
            m2 = re.match(r'(7|8|9|10|11|12|13|14|15|16|17|18|19)時台.*?(7|8|9|10|11|12|13|14|15|16|17|18|19)時台.*', dl)
            if m2:
                idx2 = dl.find(m2.group(2) + '時台', 5)
                left = dl[:idx2].strip()
                right = dl[idx2:].strip()
            else:
                mid = len(dl) // 2
                left = dl[:mid].strip()
                right = dl[mid:].strip()
            left_cols = left.split(' ')
            right_cols = right.split(' ')
            # 合計値はindex=7
            in_goukei = left_cols[7] if len(left_cols) > 7 else ''
            out_goukei = right_cols[7] if len(right_cols) > 7 else ''
            in_values.append(in_goukei.replace(',', ''))
            out_values.append(out_goukei.replace(',', ''))
        # ファイル名から日付・番号・断面名を抽出
        m = re.match(r'.*([0-9]{1,2})月([0-9]{1,2})日_([0-9]+)_([A-Z])\.txt', os.path.basename(file))
        if not m:
            print('ファイル名から情報が抽出できません', flush=True)
            continue
        month, day, num, section = m.groups()
        date = f'2024{int(month):02d}{int(day):02d}'
        # in側
        if len(in_values) == 13:
            lane_in = f'Link{num}_{section}_in'
            linkid_in = lane_to_linkid.get(lane_in, 'Unknown')
            csv_rows_in = []
            for j in range(13):
                start_hour = f'{7+j:02d}00'
                end_hour = f'{8+j:02d}00'
                start_time = date + start_hour
                end_time = date + end_hour
                csv_rows_in.append([
                    start_time,
                    end_time,
                    linkid_in,
                    in_values[j],
                    lane_in
                ])
            outname_in = os.path.basename(file).replace('.txt', '_in.csv')
            outpath_in = os.path.join(csv_dir, outname_in)
            print(f'Writing: {outpath_in} ({len(csv_rows_in)} rows)', flush=True)
            with open(outpath_in, 'w', encoding='utf-8', newline='') as out:
                writer = csv.writer(out)
                writer.writerow(header)
                writer.writerows(csv_rows_in)
        # out側
        if len(out_values) == 13:
            lane_out = f'Link{num}_{section}_out'
            linkid_out = lane_to_linkid.get(lane_out, 'Unknown')
            csv_rows_out = []
            for j in range(13):
                start_hour = f'{7+j:02d}00'
                end_hour = f'{8+j:02d}00'
                start_time = date + start_hour
                end_time = date + end_hour
                csv_rows_out.append([
                    start_time,
                    end_time,
                    linkid_out,
                    out_values[j],
                    lane_out
                ])
            outname_out = os.path.basename(file).replace('.txt', '_out.csv')
            outpath_out = os.path.join(csv_dir, outname_out)
            print(f'Writing: {outpath_out} ({len(csv_rows_out)} rows)', flush=True)
            with open(outpath_out, 'w', encoding='utf-8', newline='') as out:
                writer = csv.writer(out)
                writer.writerow(header)
                writer.writerows(csv_rows_out)
except Exception as e:
    print('Error:', e, flush=True) 