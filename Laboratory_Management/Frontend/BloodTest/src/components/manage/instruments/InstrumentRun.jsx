import React, { useEffect, useMemo, useState } from "react";
import { useSearchParams } from "react-router-dom";
import { startInstrumentRun } from "../../../apis/InstrumentAPI";
import AdminLayout from "../../admin/layout/AdminLayout";
import { FiDroplet, FiCheckCircle } from "react-icons/fi";
import "./InstrumentRun.css";
import bloodCellsGif from "../../../assets/gif/red-blood-cells.gif";

const defaultResults = [
  {
    label: "Huyết Sắc Tố (Hemoglobin)",
    unit: "g/dL",
    range: "12-16 g/dL",
    value: 14.2,
  },
  { label: "Hồng Cầu (RBC)", unit: "M/µL", range: "4.5-5.5 M/µL", value: 5.1 },
  { label: "Trắng Cầu (WBC)", unit: "K/µL", range: "4.5-11 K/µL", value: 7.3 },
  {
    label: "Tiểu Cầu (Platelets)",
    unit: "K/µL",
    range: "150-400 K/µL",
    value: 245,
  },
  { label: "Glucose", unit: "mg/dL", range: "70-100 mg/dL", value: 92 },
  {
    label: "Cholesterol Tổng",
    unit: "mg/dL",
    range: "< 200 mg/dL",
    value: 185,
  },
];

const InstrumentRun = () => {
  // const navigate = useNavigate();
  const [params] = useSearchParams();
  const bookingId = params.get("bookingId");
  const bookingCode = params.get("bookingCode") || "";
  const patientName = params.get("patientName") || "";

  const [seconds, setSeconds] = useState(30);
  const [phase, setPhase] = useState("pending"); // pending -> running -> done | error
  const [message, setMessage] = useState("");
  const [progress, setProgress] = useState(0);
  const [results, setResults] = useState(defaultResults);
  const breadcrumbs = useMemo(
    () => [
      { name: "Phòng Xét Nghiệm", link: "/instruments" },
      { name: "Mô phỏng" },
    ],
    []
  );

  useEffect(() => {
    if (!bookingId) return;
    const storageKey = `instrument_run_${bookingId}`;
    const stored = localStorage.getItem(storageKey);

    if (stored) {
      try {
        const data = JSON.parse(stored);
        const elapsed = Math.floor((Date.now() - data.startTime) / 1000);
        const remaining = Math.max(0, 30 - elapsed);

        setSeconds(remaining);
        setPhase(data.phase || "pending");
        setProgress(data.progress || 0);
        setMessage(String(data.message || ""));

        if (remaining === 0 && data.phase === "pending") {
          // Timer đã hết, sẽ trigger API
        }
      } catch (e) {
        // Nếu data lỗi, reset
        setPhase("pending");
        setSeconds(60);
        setProgress(0);
        localStorage.setItem(
          storageKey,
          JSON.stringify({
            startTime: Date.now(),
            phase: "pending",
            progress: 0,
            message: "",
          })
        );
      }
    } else {
      // Lần đầu, khởi tạo
      setPhase("pending");
      setSeconds(60);
      setProgress(0);
      localStorage.setItem(
        storageKey,
        JSON.stringify({
          startTime: Date.now(),
          phase: "pending",
          progress: 0,
          message: "",
        })
      );
    }

    const interval = setInterval(() => {
      setSeconds((s) => {
        if (s <= 1) {
          clearInterval(interval);
          return 0;
        }
        return s - 1;
      });
    }, 1000);

    return () => clearInterval(interval);
  }, [bookingId]);

  // Kick off API when countdown reaches 0
  useEffect(() => {
    if (phase !== "pending" || seconds !== 0 || !bookingId) return;

    const run = async () => {
      const storageKey = `instrument_run_${bookingId}`;
      const stored = localStorage.getItem(storageKey);
      const originalStartTime = stored
        ? JSON.parse(stored).startTime
        : Date.now();

      try {
        setPhase("running");
        setProgress(35);

        // Cập nhật localStorage - GIỮ NGUYÊN startTime gốc
        localStorage.setItem(
          storageKey,
          JSON.stringify({
            startTime: originalStartTime,
            phase: "running",
            progress: 35,
            message: "",
          })
        );

        const data = await startInstrumentRun(bookingId);
        const status = (data?.status || data?.Status || "")
          .toString()
          .toUpperCase();
        const msg = String(data?.message || data?.Message || "");
        setMessage(msg);

        // Simulate progress finishing quickly after response
        setProgress(100);
        if (status === "COMPLETED") {
          setPhase("done");
        } else {
          setPhase("error");
          localStorage.setItem(
            storageKey,
            JSON.stringify({
              startTime: originalStartTime,
              phase: "error",
              progress: 100,
              message: msg,
            })
          );
        }
      } catch (e) {
        let msg = "Không thể khởi chạy thiết bị. Vui lòng thử lại.";

        if (typeof e === "string") {
          msg = e;
        } else if (e?.response?.data?.message) {
          msg = String(e.response.data.message);
        } else if (e?.message) {
          msg = String(e.message);
        } else if (e) {
          try {
            msg = JSON.stringify(e);
          } catch {
            msg = "Không thể khởi chạy thiết bị. Vui lòng thử lại.";
          }
        }

        setMessage(msg);
        setPhase("error");
        localStorage.setItem(
          storageKey,
          JSON.stringify({
            startTime: originalStartTime,
            phase: "error",
            progress: 0,
            message: msg,
          })
        );
      }
    };

    run();
  }, [seconds, phase, bookingId]);

  useEffect(() => {
    if (phase === "done" && bookingId) {
      const storageKey = `instrument_run_${bookingId}`;
      localStorage.removeItem(storageKey);
    }
  }, [phase, bookingId]);

  return (
    <AdminLayout pageTitle="Phòng Xét Nghiệm Tự Động" breadcrumbs={breadcrumbs}>
      <div className="ir-container">
        <div className="ir-header">
          <h1 className="ir-title">
            <FiDroplet className="ir-title-icon" /> Kết Quả Xét Nghiệm
          </h1>
          <p className="ir-sub">Dưới đây là kết quả xét nghiệm</p>
          <div className="ir-badge">
            <div className="ir-badge-inner">
              <div className="ir-badge-title">Bệnh nhân</div>
              <div className="ir-badge-name">{patientName || "—"}</div>
              <div className="ir-badge-code">
                Mã đặt lịch: {bookingCode || bookingId}
              </div>
            </div>
          </div>
        </div>

        {phase === "pending" && (
          <div className="ir-card waiting">
            <div className="ir-wait-inner">
              {/* <img
                src={bloodCellsGif}
                alt="Blood cells animation"
                style={{
                  width: "150px",
                  marginBottom: "20px",
                  display: "flex",
                  justifyContent: "center",
                  alignItems: "center",
                }}
              /> */}
              <h2 className="ir-wait-title">Đang Xử Lý Mẫu</h2>
              <p className="ir-wait-desc">Xét nghiệm sẽ tự động bắt đầu sau:</p>
              <div className="ir-countdown">{`${String(
                Math.floor(seconds / 60)
              ).padStart(1, "0")}:${String(seconds % 60).padStart(
                2,
                "0"
              )}`}</div>
              <div className="ir-note">
                Đang trong quá trình lấy mẫu và xét nghiệm,Vui lòng đợi trong
                giây lát
              </div>
            </div>
            <div className="ir-hint">
              💡 Kết quả xét nghiệm sẽ tự động gửi sau khi hết khoảng thời gian
              đợi ở trên.
            </div>
          </div>
        )}

        {(phase === "running" || phase === "done") && (
          <div className="ir-card results">
            <div className="ir-progress-head">
              <span className="ir-progress-label">Tiến Độ Xét Nghiệm</span>
              <span className="ir-progress-percent">
                {progress === 100 ? "100%" : `${progress}%`}
              </span>
            </div>
            <div className="ir-progress">
              <div className="bar" style={{ width: `${progress}%` }} />
            </div>

            <div className="ir-grid">
              {results.map((r, idx) => (
                <div key={idx} className="ir-metric">
                  <div className="ir-metric-top">
                    <div className="ir-metric-title">{r.label}</div>
                    <FiCheckCircle
                      className={`ir-metric-icon ${
                        phase === "done" ? "done" : "pending"
                      }`}
                    />
                  </div>
                  <div className="ir-metric-range">Chuẩn: {r.range}</div>
                  <div className="ir-metric-value">
                    {r.value} <span>{r.unit}</span>
                  </div>
                </div>
              ))}
              {phase === "done" && (
                <div className="ir-complete">
                  <div className="ir-complete-row">
                    <FiCheckCircle className="ir-complete-icon" />
                    <div>
                      <div className="ir-complete-title">
                        Hoàn Thành Xét Nghiệm
                      </div>
                      <div className="ir-complete-desc">
                        {
                          "Xét Nghiệm Thành Công, Kết quả xét nghiệm đã được trả về lịch sử xét nghiệm"
                        }
                      </div>
                    </div>
                  </div>
                </div>
              )}
            </div>
            <div className="ir-footer-hint">
              💡 Kết quả xét nghiệm tự động chạy sau 30 giây kể từ khi check-in
            </div>
          </div>
        )}

        {phase === "error" && (
          <div className="ir-card error">
            <div className="ir-complete-icon">⚠️</div>
            <div className="ir-complete-title">
              Không thể hoàn tất xét nghiệm
            </div>
            <div className="ir-complete-desc">
              {String(message || "Vui lòng thử lại hoặc liên hệ quản trị.")}
            </div>
          </div>
        )}
      </div>
    </AdminLayout>
  );
};

export default InstrumentRun;
