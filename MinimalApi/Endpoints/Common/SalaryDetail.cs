namespace MinimalApi.Endpoints.Common
{
    public class SalaryDetail
    {
        /// <summary>
        /// 获取薪资明细
        /// </summary>
        /// <returns></returns>
        public static List<SalaryItem>? GetSalary()
        {
            var path = AppDomain.CurrentDomain.BaseDirectory + "SalaryUrls";
            IList<string>? list = FileIOHelper.FindDirectory(path);
            if (list == null)
            {
                return null;
            }

            List<SalaryItem> salaryItems = new List<SalaryItem>();
            int lastdate = int.Parse(DateTime.Now.AddMonths(-1).Year.ToString() + DateTime.Now.AddMonths(-1).Month.ToString().PadLeft(2, '0'));
            foreach (string file in list)
            {
                Rootobject? rootobject = Newtonsoft.Json.JsonConvert.DeserializeObject<Rootobject>(file);

                if (rootobject == null)
                {
                    continue;
                }
                if (rootobject.salaryList == null)
                {
                    continue;
                }
                SalaryItem? salaryItem = new SalaryItem();
                salaryItem.datacyear = int.Parse(rootobject.salaryList?.wa_datacyear?.content ?? "0");
                salaryItem.datacperiod = int.Parse(rootobject.salaryList?.wa_datacperiod?.content ?? "0");
                var fdate = int.Parse(salaryItem.datacyear.ToString() + salaryItem.datacperiod.ToString().PadLeft(2, '0'));
                if (lastdate != fdate)
                    continue;

                salaryItem.dataf_32 = decimal.Parse(rootobject.salaryList?.wa_dataf_32?.content ?? "0");

                salaryItem.dataf_131 = double.Parse(rootobject.salaryList?.wa_dataf_131?.content ?? "0");
                salaryItem.dataf_134 = double.Parse(rootobject.salaryList?.wa_dataf_134?.content ?? "0");
                salaryItem.dataf_40 = decimal.Parse(rootobject.salaryList?.wa_dataf_40?.content ?? "0");
                salaryItem.dataf_95 = decimal.Parse(rootobject.salaryList?.wa_dataf_95?.content ?? "0");

                decimal? dataf_94 = 0.00M;

                if (fdate <= 202110)
                {
                    dataf_94 = 13000M * 0.15M;
                }
                else if (fdate >= 202111 && fdate <= 202303)
                {
                    dataf_94 = 14000.00M * 0.15M;
                }
                else if (fdate >= 202304)
                {
                    dataf_94 = 14500.00M * 0.15M;
                }

                salaryItem.dataf_94 = dataf_94;

                salaryItem.dataf_96 = salaryItem.dataf_95 - salaryItem.dataf_94;

                if (salaryItem.dataf_94.GetValueOrDefault() != 0)
                {
                    // 先计算百分比数值，再格式化保留 3 位小数，最后拼接 %
                    decimal percentValue = ((salaryItem.dataf_96 ?? 0m) / salaryItem.dataf_94.Value) * 100m;
                    salaryItem.dataf_97 = percentValue.ToString("F3") + "%";
                }
                else
                {
                    // 分母为 null 或 0 时的默认处理
                    salaryItem.dataf_97 = "0.000%";
                }

                if (rootobject.salaryList?.wa_dataf_63 != null)
                {
                    salaryItem.dataf_63 = decimal.Parse(rootobject.salaryList?.wa_dataf_63?.content ?? "0");
                }
                else
                {
                    salaryItem.dataf_63 = 0;
                }

                salaryItem.dataf_79 = decimal.Parse(rootobject.salaryList?.wa_dataf_79?.content ?? "0");
                salaryItem.dataf_158 = decimal.Parse(rootobject.salaryList?.wa_dataf_158?.content ?? "0");
                salaryItem.dataf_159 = decimal.Parse(rootobject.salaryList?.wa_dataf_159?.content ?? "0");
                if (rootobject.salaryList?.wa_dataf_5 != null)
                {
                    salaryItem.dataf_5 = decimal.Parse(rootobject.salaryList?.wa_dataf_5?.content ?? "0");
                }
                else
                {
                    salaryItem.dataf_5 = 0;
                }

                salaryItem.dataf_3 = decimal.Parse(rootobject.salaryList?.wa_dataf_3?.content ?? "0");

                salaryItem.dataf_157 = decimal.Parse(rootobject.salaryList?.wa_dataf_157?.content ?? "0");
                salaryItem.dataf_162 = decimal.Parse(rootobject.salaryList?.wa_dataf_162?.content ?? "0");

                var totalDeduction = -salaryItem.dataf_96 + salaryItem.dataf_63 + salaryItem.dataf_158 + salaryItem.dataf_5;
                salaryItem.dataf_163 = totalDeduction;
                salaryItems.Add(salaryItem);
            }
            return salaryItems;
        }
    }
}
