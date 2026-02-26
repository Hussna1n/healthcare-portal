import { useState } from 'react';
import { Calendar, Clock, Video, MapPin, Phone, CheckCircle, XCircle, AlertCircle } from 'lucide-react';
import { format, isPast } from 'date-fns';
import { useGetAppointmentsQuery, useCancelAppointmentMutation } from '../store/api';

type AppointmentStatus = 'scheduled' | 'confirmed' | 'in_progress' | 'completed' | 'cancelled';
type AppointmentType = 'in-person' | 'video' | 'phone';

interface Appointment {
  id: number; scheduledAt: string; durationMinutes: number;
  status: AppointmentStatus; type: AppointmentType; notes?: string;
  doctor: { id: number; firstName: string; lastName: string; specialization: string; avatarUrl?: string; consultationFee: number };
}

const STATUS_CONFIG: Record<AppointmentStatus, { label: string; color: string; icon: any }> = {
  scheduled: { label: 'Scheduled', color: 'bg-blue-50 text-blue-600', icon: Clock },
  confirmed: { label: 'Confirmed', color: 'bg-green-50 text-green-600', icon: CheckCircle },
  in_progress: { label: 'In Progress', color: 'bg-yellow-50 text-yellow-600', icon: AlertCircle },
  completed: { label: 'Completed', color: 'bg-gray-50 text-gray-600', icon: CheckCircle },
  cancelled: { label: 'Cancelled', color: 'bg-red-50 text-red-400', icon: XCircle },
};

const TYPE_ICONS = { 'in-person': MapPin, video: Video, phone: Phone };

const TABS: AppointmentStatus[] = ['scheduled', 'confirmed', 'completed', 'cancelled'];

export default function AppointmentsPage() {
  const [activeTab, setActiveTab] = useState<AppointmentStatus>('scheduled');
  const { data = [], isLoading } = useGetAppointmentsQuery({ status: activeTab });
  const [cancelAppointment] = useCancelAppointmentMutation();

  return (
    <div className="max-w-3xl mx-auto px-4 py-8">
      <div className="flex justify-between items-center mb-8">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">My Appointments</h1>
          <p className="text-gray-500 text-sm">Manage your healthcare appointments</p>
        </div>
        <a href="/book" className="bg-indigo-600 text-white px-4 py-2 rounded-lg text-sm font-medium hover:bg-indigo-700">
          Book Appointment
        </a>
      </div>

      {/* Tabs */}
      <div className="flex gap-2 mb-6 p-1 bg-gray-100 rounded-xl">
        {TABS.map(tab => (
          <button key={tab} onClick={() => setActiveTab(tab)}
            className={`flex-1 py-2 px-3 rounded-lg text-sm font-medium capitalize transition-all ${activeTab === tab ? 'bg-white shadow-sm text-gray-900' : 'text-gray-500 hover:text-gray-700'}`}>
            {tab.replace('_', ' ')}
          </button>
        ))}
      </div>

      {isLoading ? (
        <div className="space-y-4">
          {[1, 2, 3].map(i => <div key={i} className="h-32 bg-gray-100 rounded-xl animate-pulse" />)}
        </div>
      ) : data.length === 0 ? (
        <div className="text-center py-16 text-gray-400">
          <Calendar size={40} className="mx-auto mb-3 opacity-30" />
          <p>No {activeTab} appointments</p>
        </div>
      ) : (
        <div className="space-y-4">
          {(data as Appointment[]).map(appt => {
            const { label, color, icon: StatusIcon } = STATUS_CONFIG[appt.status];
            const TypeIcon = TYPE_ICONS[appt.type];
            const past = isPast(new Date(appt.scheduledAt));

            return (
              <div key={appt.id} className={`bg-white rounded-xl border border-gray-100 shadow-sm p-5 ${past && appt.status === 'scheduled' ? 'opacity-70' : ''}`}>
                <div className="flex gap-4">
                  <img src={appt.doctor.avatarUrl || `https://ui-avatars.com/api/?name=${appt.doctor.firstName}+${appt.doctor.lastName}&background=6366f1&color=fff`}
                    className="w-14 h-14 rounded-xl object-cover" alt={appt.doctor.firstName} />
                  <div className="flex-1">
                    <div className="flex justify-between items-start">
                      <div>
                        <p className="font-semibold text-gray-900">Dr. {appt.doctor.firstName} {appt.doctor.lastName}</p>
                        <p className="text-sm text-indigo-600">{appt.doctor.specialization}</p>
                      </div>
                      <span className={`flex items-center gap-1 text-xs font-medium px-2.5 py-1 rounded-full ${color}`}>
                        <StatusIcon size={12} />{label}
                      </span>
                    </div>

                    <div className="flex flex-wrap gap-4 mt-3 text-sm text-gray-500">
                      <span className="flex items-center gap-1.5">
                        <Calendar size={14} />
                        {format(new Date(appt.scheduledAt), 'MMM dd, yyyy')}
                      </span>
                      <span className="flex items-center gap-1.5">
                        <Clock size={14} />
                        {format(new Date(appt.scheduledAt), 'HH:mm')} ({appt.durationMinutes} min)
                      </span>
                      <span className="flex items-center gap-1.5 capitalize">
                        <TypeIcon size={14} />{appt.type}
                      </span>
                    </div>

                    {appt.notes && <p className="text-xs text-gray-400 mt-2 italic">"{appt.notes}"</p>}

                    {appt.status === 'scheduled' && !past && (
                      <div className="flex gap-2 mt-3">
                        {appt.type === 'video' && (
                          <button className="flex items-center gap-1.5 bg-green-50 text-green-600 text-xs font-medium px-3 py-1.5 rounded-lg hover:bg-green-100">
                            <Video size={13} />Join Call
                          </button>
                        )}
                        <button onClick={() => cancelAppointment(appt.id)}
                          className="flex items-center gap-1.5 bg-red-50 text-red-500 text-xs font-medium px-3 py-1.5 rounded-lg hover:bg-red-100">
                          <XCircle size={13} />Cancel
                        </button>
                      </div>
                    )}
                  </div>
                </div>
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
}
